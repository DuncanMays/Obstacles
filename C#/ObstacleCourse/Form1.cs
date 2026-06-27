using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ObstacleCourse
{

    public static class Globals
    {
        public static List<Agent> agents = [];
        public static List<Agent> live_agents = [];
        public static List<Obstacle> obstacles = [];
        public static Random rnd = new Random();
        public static int top_border;
        public static int bottom_border;
        public static int window_slide = 0;
    }

    public partial class Form1 : Form
    {

        int best_distance = 0;
        int generation = 0;
        Brain brain;
        PPOTrainer trainer;

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.BackColor = System.Drawing.Color.White;
            this.Height = 800;

            Globals.top_border = 100;
            Globals.bottom_border = this.Height - 100;

            this.brain = new Brain();
            this.trainer = new PPOTrainer(this.brain);

            SpawnAgents();
            MakeCourse.intro_obstacles(500);
            MakeCourse.generated_up_to = 5500;
        }

        private void SpawnAgents()
        {
            for (int i = 0; i < 50; i++)
            {
                new Agent(100, Globals.rnd.Next(Globals.top_border + 100, Globals.bottom_border - 100), 5, this.brain);
            }
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void WindowTimer_Tick(object sender, EventArgs e)
        {
            foreach (Obstacle o in Globals.obstacles)
            {
                o.step();
            }

            List<Agent> iter_agents = new List<Agent>(Globals.live_agents.Count);
            Globals.live_agents.ForEach((a) => { iter_agents.Add(a); });

            foreach (Agent a in iter_agents)
            {
                a.step();

                if ((a.y < Globals.top_border) | (a.y > Globals.bottom_border))
                {
                    a.die();
                }

                foreach (Obstacle o in Globals.obstacles)
                {
                    if (o.check_collision(a.x, a.y))
                    {
                        a.die();
                    }
                }

                if (a.x > this.best_distance)
                {
                    this.best_distance = a.x;
                }
            }

            MakeCourse.generate_ahead(this.best_distance);

            int cull_margin = 1500;
            if (Globals.live_agents.Count > 0)
            {
                int min_x = Globals.live_agents.Min(a => a.x);
                Globals.obstacles.RemoveAll(o => o.x + o.cull_radius < min_x - cull_margin);
            }

            if (Globals.live_agents.Count == 0)
            {
                EndGeneration();
            }

            if (this.best_distance > this.Width - 200)
            {
                Globals.window_slide = this.Width - this.best_distance - 200;
            }

            this.Invalidate();
        }

        private void EndGeneration()
        {
            int max_dist = 0;
            float total_reward = 0;
            int total_steps = 0;

            foreach (Agent a in Globals.agents)
            {
                if (a.x > max_dist) max_dist = a.x;
                total_reward += a.trajectory.rewards.Sum();
                total_steps += a.trajectory.rewards.Count;
            }

            float mean_reward = total_steps > 0 ? total_reward / Globals.agents.Count : 0;

            this.trainer.train(Globals.agents);
            this.generation++;

            this.Text = $"Obstacle Course — Gen {this.generation} | Max dist: {max_dist} | Mean reward: {mean_reward:F2}";

            Globals.agents.Clear();
            Globals.live_agents.Clear();
            Globals.obstacles.Clear();
            Globals.window_slide = 0;
            this.best_distance = 0;

            MakeCourse.generated_up_to = 0;
            MakeCourse.intro_obstacles(500);
            MakeCourse.generated_up_to = 5500;

            SpawnAgents();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            foreach (Agent a in Globals.agents)
            {
                a.draw(e);
            }

            foreach (Obstacle o in Globals.obstacles)
            {
                o.draw(e);
                if (o is Tracker t) { t.draw_zone(e); }
            }

            DrawBorder(e, Globals.top_border);
            DrawBorder(e, Globals.bottom_border);
        }

        private void DrawBorder(PaintEventArgs e, int y)
        {
            Pen pen = new Pen(Color.FromArgb(255, 0, 0, 0), 10);
            e.Graphics.DrawLine(pen, 0, y, this.Width, y);
        }
    }
}
