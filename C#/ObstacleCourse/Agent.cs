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
    public class Agent
    {
        public int x;
        public int y;
        public double theta;
        double ta;
        float speed;
        double[][] vertices;
        bool alive = true;
        Color colour = Color.Red;
        Eye[] eyes;
        Brain brain;

        public Agent(int x, int y, float speed)
        {
            this.x = x;
            this.y = y;
            this.theta = 0;
            this.speed = speed;
            this.brain = new Brain();

            //the corners that make up the shape of the agent on screen, expressed as a  tuple of radius and angle
            this.vertices = [[0, 0], [20, 0.9 * Math.PI], [15, Math.PI], [20, 1.1 * Math.PI]];

            int vision_range = 300;
            int num_eyes = 5;
            double eye_spread = 0.8;

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

            //get sensory input from the eyes
            double[] d = new double[5];
            for (int i = 0; i < this.eyes.Length; i++)
            {
                d[i] = this.eyes[i].get_distance(this) / this.eyes[i].max_dist;
            }

            //make decisions with eye input
            torch.Tensor y = this.brain.infer(d, this.theta);

            //hand-coded rules
            if (d[0] < 0.3) { this.theta = this.theta - 0.2; }
            if (d[2] < 0.15) { this.speed = 1; } else { this.speed = 3; }
            if (d[4] < 0.3) { this.theta = this.theta + 0.2; }

            this.ta = this.ta + 0.0005 * (Globals.rnd.Next(0, 100) - 50);
            this.ta = Math.Max(Math.Min(0.05, this.ta), -0.05);
            this.theta = this.theta + ta;
            this.theta = this.theta % (2 * Math.PI);

            //Move in the direction theta by the distance speed
            this.x = this.x + (int)(this.speed * Math.Cos(this.theta));
            this.y = this.y + (int)(this.speed * Math.Sin(this.theta));
        }

        public void die()
        {
            this.alive = false;
            //removes this agent from the live agents list
            Globals.live_agents.Remove(this);
            //changes the way dead agents show up on screen
            this.colour = Color.LightGray;
        }

        public void draw(PaintEventArgs e)
        {
            //if not on the screen, don't draw
            if (this.x + Globals.window_slide < -10) { return; }

            Pen pen = new Pen(this.colour, 3);
            Point[] draw_vertices = new Point[this.vertices.Length];

            //draw all the vertices
            for (int i = 0; i < this.vertices.Length; i++)
            {
                double[] v = this.vertices[i];
                double r = v[0];
                double psi = v[1];

                //rotate the vertex by the angle of the agent, then mod by 2PI
                psi = (psi + this.theta) % (2 * Math.PI);

                //convert into cartesian and translate by the agents position, as well as window_slide
                int x = this.x + (int)(r * Math.Cos(psi)) + Globals.window_slide;
                int y = this.y + (int)(r * Math.Sin(psi));

                draw_vertices[i] = new Point(x, y);
            }

            e.Graphics.DrawPolygon(pen, draw_vertices);

            //if (this.alive)
            //{
            //    foreach (Eye eye in this.eyes) { eye.draw(e, this); }
            //}
        }
    }

    public class Brain
    {

        Linear lin1;
        Linear lin2;
        Sequential seq;
        OptimizerHelper optimizer;

        public Brain()
        {
            this.lin1 = Linear(6, 100);
            this.lin2 = Linear(100, 2);
            this.seq = Sequential(("lin1", this.lin1), ("relu1", ReLU()), ("drop1", Dropout(0.05)), ("lin2", this.lin2), ("tanh", Tanh()));

            this.optimizer = torch.optim.Adam(this.seq.parameters());
        }

        public torch.Tensor infer(double[] eye_input, double theta)
        {
            float[] o = new float[eye_input.Length + 1];

            for (int i = 0; i < eye_input.Length; i++)
            {
                o[i] = (float)eye_input[i];
            }

            o[eye_input.Length] = (float)(theta / (2 * Math.PI) );

            torch.Tensor t = torch.from_array(o);
            torch.Tensor a = this.seq.forward(t);

            return a;
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

            //draw a line out of the front of the agent to the closest obstacle
            Color c = Color.Yellow;

            if (d == this.max_dist)
            {
                c = Color.Red;
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
                if ((o.x - a.x) * Math.Cos(a.theta) + (o.y - a.y) * Math.Sin(a.theta) < 0) { continue; }
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
            //if (Math.Sin(a.theta + this.psi) == 0) { return this.max_dist; }
            if ((border_y - a.y) * Math.Sin(a.theta + this.psi) <= 0) { return this.max_dist; }
            return Math.Abs((border_y - a.y) / Math.Sin(a.theta + this.psi));
        }

        public double get_distance_to_obstacle(Agent a, Obstacle o)
        {
            //these two doubles are elements of a cartesian vector representing the direction the eye is looking in, of length one
            double eye_x = Math.Cos(a.theta + this.psi);
            double eye_y = Math.Sin(a.theta + this.psi);

            //these two doubles are elements of a cartesian vector from the agent to the centre of the obstacle
            double x_trans = (o.x - a.x);
            double y_trans = (o.y - a.y);
            double h = Math.Sqrt(Math.Pow(x_trans, 2) + Math.Pow(y_trans, 2));

            //using the dot product, calculate the angle between the eye and the vector to the obstacle
            double cosine = (x_trans * eye_x + y_trans * eye_y) / h;

            double alpha = Math.Acos(cosine);
            if (y_trans < 0) { alpha = 2 * Math.PI - alpha; }

            return h * cosine - Math.Sqrt(Math.Pow(o.rad, 2) - Math.Pow(h * Math.Sin(alpha), 2));
        }
    }
}
