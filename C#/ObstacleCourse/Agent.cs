using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch.nn;

namespace ObstacleCourse
{
    public class Trajectory
    {
        public List<float[]> states = new();
        public List<float[]> actions = new();
        public List<float> log_probs = new();
        public List<float> rewards = new();
        public List<float> values = new();
    }

    public class Agent
    {
        public int x;
        public int y;
        public double theta;
        public float speed;
        double[][] vertices;
        bool alive = true;
        Color colour = Color.Red;
        Eye[] eyes;
        Brain brain;
        public Trajectory trajectory;
        public float[] last_state;

        const float max_speed = 8f;
        const float min_speed = 1f;
        const float max_steering = 0.15f;
        const float max_accel = 0.5f;

        int stagnation_counter = 0;
        int last_x;
        const int stagnation_limit = 500;

        public Agent(int x, int y, float speed, Brain brain)
        {
            this.x = x;
            this.y = y;
            this.last_x = x;
            this.theta = 0;
            this.speed = speed;
            this.brain = brain;
            this.trajectory = new Trajectory();

            this.vertices = [[0, 0], [20, 0.9 * Math.PI], [15, Math.PI], [20, 1.1 * Math.PI]];

            int vision_range = 300;
            int num_eyes = 11;
            double eye_spread = 2.5;

            this.eyes = new Eye[num_eyes];

            for (int i = 0; i < num_eyes; i++)
            {
                double interval = 2 * eye_spread / (num_eyes - 1);
                eyes[i] = new Eye(eye_spread - i * interval, vision_range);
            }

            Globals.agents.Add(this);
            Globals.live_agents.Add(this);
        }

        public void step()
        {
            if (!this.alive) { return; }

            int prev_x = this.x;

            float[] state = new float[this.eyes.Length + 2];
            for (int i = 0; i < this.eyes.Length; i++)
            {
                state[i] = (float)(this.eyes[i].get_distance(this) / this.eyes[i].max_dist);
            }
            state[this.eyes.Length] = (float)(this.theta / (2 * Math.PI));
            state[this.eyes.Length + 1] = this.speed / max_speed;

            float[] state_suffix = this.last_state ?? new float[state.Length];
            float[] full_state = state.Concat(state_suffix).ToArray();

            var (action, log_prob, value) = this.brain.act(full_state);

            this.last_state = state;

            this.theta += action[0] * max_steering;
            this.theta = this.theta % (2 * Math.PI);
            this.speed += action[1] * max_accel;
            this.speed = Math.Clamp(this.speed, min_speed, max_speed);

            this.x = this.x + (int)(this.speed * Math.Cos(this.theta));
            this.y = this.y + (int)(this.speed * Math.Sin(this.theta));

            float reward = (this.x - prev_x) * 0.01f;

            this.trajectory.states.Add(full_state);
            this.trajectory.actions.Add(action);
            this.trajectory.log_probs.Add(log_prob);
            this.trajectory.values.Add(value);
            this.trajectory.rewards.Add(reward);

            if (this.x > this.last_x)
            {
                this.last_x = this.x;
                this.stagnation_counter = 0;
            }
            else
            {
                this.stagnation_counter++;
            }

            if (this.stagnation_counter > stagnation_limit)
            {
                this.die();
            }
        }

        public void die()
        {
            if (!this.alive) return;
            this.alive = false;
            if (this.trajectory.rewards.Count > 0)
            {
                this.trajectory.rewards[^1] -= 5.0f;
            }
            Globals.live_agents.Remove(this);
            this.colour = Color.LightGray;
        }

        public void draw(PaintEventArgs e)
        {
            if (this.x + Globals.window_slide < -10) { return; }

            Pen pen = new Pen(this.colour, 3);
            Point[] draw_vertices = new Point[this.vertices.Length];

            for (int i = 0; i < this.vertices.Length; i++)
            {
                double[] v = this.vertices[i];
                double r = v[0];
                double psi = v[1];

                psi = (psi + this.theta) % (2 * Math.PI);

                int x = this.x + (int)(r * Math.Cos(psi)) + Globals.window_slide;
                int y = this.y + (int)(r * Math.Sin(psi));

                draw_vertices[i] = new Point(x, y);
            }

            e.Graphics.DrawPolygon(pen, draw_vertices);

            // if (this.alive)
            // {
            //    foreach (Eye eye in this.eyes) { eye.draw(e, this); }
            // }
        }
    }

    public class Brain
    {
        Sequential backbone;
        Linear actor_head;
        Linear critic_head;
        Parameter log_std;
        public OptimizerHelper optimizer;

        public Brain()
        {
            this.backbone = Sequential(
                ("fc1", Linear(26, 128)),
                ("relu1", ReLU()),
                ("fc2", Linear(128, 64)),
                ("relu2", ReLU())
            );
            this.actor_head = Linear(64, 2);
            this.critic_head = Linear(64, 1);
            this.log_std = new Parameter(torch.zeros(2));

            var all_params = this.backbone.parameters()
                .Concat(this.actor_head.parameters())
                .Concat(this.critic_head.parameters())
                .Append(this.log_std);
            this.optimizer = torch.optim.Adam(all_params, lr: 3e-4);
        }

        public (float[] action, float log_prob, float value) act(float[] state)
        {
            using (torch.no_grad())
            {
                var input = torch.from_array(state);
                var features = this.backbone.forward(input);
                var means = this.actor_head.forward(features);
                var std = this.log_std.exp();
                var dist = torch.distributions.Normal(means, std);
                var sampled = dist.sample();
                var log_prob = dist.log_prob(sampled).sum();
                var value = this.critic_head.forward(features).squeeze(-1);

                float[] action = new float[2];
                action[0] = sampled[0].item<float>();
                action[1] = sampled[1].item<float>();

                return (action, log_prob.item<float>(), value.item<float>());
            }
        }

        public (torch.Tensor new_log_probs, torch.Tensor values, torch.Tensor entropy) evaluate(torch.Tensor states, torch.Tensor actions)
        {
            var features = this.backbone.forward(states);
            var means = this.actor_head.forward(features);
            var std = this.log_std.exp().expand_as(means);
            var dist = torch.distributions.Normal(means, std);
            var new_log_probs = dist.log_prob(actions).sum(-1);
            var entropy = dist.entropy().sum(-1);
            var values = this.critic_head.forward(features).squeeze(-1);

            return (new_log_probs, values, entropy);
        }

        public IEnumerable<Parameter> parameters()
        {
            return this.backbone.parameters()
                .Concat(this.actor_head.parameters())
                .Concat(this.critic_head.parameters())
                .Append(this.log_std);
        }
    }

    public class Eye
    {
        public double psi = 0;
        public int max_dist = 300;
        public Eye(double psi, int max_dist)
        {
            this.psi = psi;
            this.max_dist = max_dist;
        }

        public void draw(PaintEventArgs e, Agent a)
        {
            double d = this.get_distance(a);

            Color c = Color.Black;

            if (d == this.max_dist)
            {
                c = Color.LightGray;
            }

            Pen pen2 = new Pen(c, 3);
            e.Graphics.DrawLine(pen2, a.x + Globals.window_slide, a.y, a.x + (int)(d * Math.Cos(a.theta + this.psi)) + Globals.window_slide, a.y + (int)(d * Math.Sin(a.theta + this.psi)));
        }

        public double get_distance(Agent a)
        {
            double min_d = this.max_dist;
            double d;

            foreach (Obstacle o in Globals.obstacles)
            {
                //if the obstacle is behind the eye, skip
                if ((o.x - a.x) * Math.Cos(this.psi) + (o.y - a.y) * Math.Sin(this.psi) < 0) { continue; }
                //if the obstacle is further than the eye can see, skip
                if (Math.Sqrt(Math.Pow(a.x - o.x, 2) + Math.Pow(a.y - o.y, 2)) > this.max_dist + o.rad) { continue; }

                d = Math.Abs(this.get_distance_to_obstacle(a, o));
                if (d < min_d) { min_d = d; }
            }

            d = this.get_disance_to_border(a, Globals.top_border);
            if (d < min_d) { min_d = d; }
            d = this.get_disance_to_border(a, Globals.bottom_border);
            if (d < min_d) { min_d = d; }

            return min_d;
        }

        public double get_disance_to_border(Agent a, int border_y)
        {
            if ((border_y - a.y) * Math.Sin(a.theta + this.psi) <= 0) { return this.max_dist; }
            return Math.Abs((border_y - a.y) / Math.Sin(a.theta + this.psi));
        }

        public double get_distance_to_obstacle(Agent a, Obstacle o)
        {
            double eye_x = Math.Cos(a.theta + this.psi);
            double eye_y = Math.Sin(a.theta + this.psi);

            double x_trans = (o.x - a.x);
            double y_trans = (o.y - a.y);
            double h = Math.Sqrt(Math.Pow(x_trans, 2) + Math.Pow(y_trans, 2));

            double cosine = (x_trans * eye_x + y_trans * eye_y) / h;

            double alpha = Math.Acos(cosine);
            if (y_trans < 0) { alpha = 2 * Math.PI - alpha; }

            return h * cosine - Math.Sqrt(Math.Pow(o.rad, 2) - Math.Pow(h * Math.Sin(alpha), 2));
        }
    }
}
