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

        public Form1()
        {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.BackColor = System.Drawing.Color.White;
            this.Height = 800;

            Globals.top_border = 100;
            Globals.bottom_border = this.Height - 100;

            for (int i = 0; i < 100; i++)
            {
                new Agent(100, Globals.rnd.Next(Globals.top_border + 100, Globals.bottom_border - 100), 5);
            }

            MakeCourse.intro_obstacles(500);

            //MakeCourse.generate_obstacles(2500);
        }

        private void Form1_Load(object sender, EventArgs e) { }

        private void WindowTimer_Tick(object sender, EventArgs e)
        {
            //updates obstacles
            foreach (Obstacle o in Globals.obstacles)
            {
                o.step();
            }

            //since live_agents might change during the iteration (agents might die) so we need to iterate over a copy
            List<Agent> iter_agents = new List<Agent>(Globals.live_agents.Count);
            Globals.live_agents.ForEach((a) => { iter_agents.Add(a); });

            foreach (Agent a in iter_agents)
            {
                //updates agents
                a.step();

                //checks if any agents are out of bounds
                if ((a.y < Globals.top_border) | (a.y > Globals.bottom_border))
                {
                    a.die();
                }

                //checks if any agents are colliding with an obstacle
                foreach (Obstacle o in Globals.obstacles)
                {
                    if (o.check_collision(a.x, a.y))
                    {
                        a.die();
                    }
                }

                //records the furthest ahead agent
                if (a.x > this.best_distance)
                {
                    this.best_distance = a.x;
                }
            }

            //Updates window_slide
            if (this.best_distance > this.Width - 200)
            {
                Globals.window_slide = this.Width - this.best_distance - 200;
            }

            //triggers this.Form1_Paint
            this.Invalidate();
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
