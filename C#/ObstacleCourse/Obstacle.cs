using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObstacleCourse
{
    public class Obstacle
    {

        public int x;
        public int y;
        public int rad;
        protected Color colour = Obstacle.get_rnd_colour();

        public Obstacle(int x, int y, int rad, Color c)
        {
            this.x = x;
            this.y = y;
            this.rad = rad;
            this.colour = c;

            Globals.obstacles.Add(this);
        }

        public static Color get_rnd_colour()
        {
            Color[] colours = [Color.Red, Color.Blue, Color.Yellow, Color.Green, Color.Orange, Color.Purple, Color.Cyan, Color.Beige, Color.Silver, Color.Gold, Color.Fuchsia, Color.Magenta, Color.DarkGoldenrod, Color.LightCoral];
            int i = Globals.rnd.Next(0, colours.Length);
            return colours[i];
        }

        public virtual int cull_radius { get { return this.rad; } }

        public virtual void step() { }

        public void draw(PaintEventArgs e)
        {
            //if not on the screen, don't draw
            if (this.x + Globals.window_slide < -this.rad) { return; }

            SolidBrush brush = new SolidBrush(this.colour);
            e.Graphics.FillEllipse(brush, x - this.rad + Globals.window_slide, y - this.rad, 2 * this.rad, 2 * this.rad);
        }

        public bool check_collision(int x, int y)
        {
            double distance = Math.Sqrt(Math.Pow(this.x - x, 2) + Math.Pow(this.y - y, 2));
            return distance <= this.rad;
        }
    }

    public partial class Slider : Obstacle
    {

        double theta;
        double theta_speed;
        int bandwidth;
        int start_y;

        public Slider(int x, int y, int rad, int bandwidth, double theta, double theta_speed, Color c) : base(x, y, rad, c)
        {
            this.start_y = y;
            this.theta = theta;
            this.theta_speed = theta_speed;
            this.bandwidth = bandwidth;
        }

        public override void step()
        {
            this.theta = this.theta + this.theta_speed;
            this.theta = this.theta % (2 * Math.PI);
            this.y = this.start_y + (int)(this.bandwidth * Math.Sin(this.theta));
        }
    }

    public partial class Spinner : Obstacle
    {

        double theta;
        double theta_speed;
        int big_radius;
        int start_x;
        int start_y;

        public override int cull_radius { get { return this.big_radius + this.rad; } }

        public Spinner(int x, int y, int rad, int big_rad, double theta, double theta_speed, Color c) : base(x, y, rad, c)
        {
            this.start_y = y;
            this.start_x = x;
            this.theta = theta;
            this.theta_speed = theta_speed;
            this.big_radius = big_rad;
        }

        public override void step()
        {
            this.theta = this.theta + this.theta_speed;
            this.theta = this.theta % (2 * Math.PI);
            this.y = this.start_y + (int)(this.big_radius * Math.Sin(this.theta));
            this.x = this.start_x + (int)(this.big_radius * Math.Cos(this.theta));
        }
    }

    public partial class Grower : Obstacle
    {
        int min_rad;
        int max_rad;
        double theta;
        double theta_speed;

        public override int cull_radius { get { return this.max_rad; } }

        public Grower(int x, int y, int min_rad, int max_rad, double theta, double theta_speed, Color c) : base(x, y, min_rad, c)
        {
            this.min_rad = min_rad;
            this.max_rad = max_rad;
            this.theta = theta;
            this.theta_speed = theta_speed;
        }

        public override void step()
        {
            this.theta = this.theta + this.theta_speed;
            this.theta = this.theta % (2 * Math.PI);
            double t = (Math.Sin(this.theta) + 1) / 2;
            this.rad = this.min_rad + (int)(t * (this.max_rad - this.min_rad));
        }
    }

    public partial class Tracker : Obstacle
    {
        int zone_radius;
        int start_x;
        int start_y;
        double chase_speed;
        List<Agent> targets = new List<Agent>();

        public override int cull_radius { get { return this.zone_radius; } }

        public Tracker(int x, int y, int rad, int zone_radius, double chase_speed, Color c) : base(x, y, rad, c)
        {
            this.start_x = x;
            this.start_y = y;
            this.zone_radius = zone_radius;
            this.chase_speed = chase_speed;
        }

        public override void step()
        {
            targets.RemoveAll(a => !Globals.live_agents.Contains(a) || !is_in_zone(a));

            foreach (Agent a in Globals.live_agents)
            {
                if (is_in_zone(a) && !targets.Contains(a))
                {
                    targets.Add(a);
                }
            }

            if (targets.Count == 0) return;

            Agent target = targets[0];
            double dx = target.x - this.x;
            double dy = target.y - this.y;
            double dist = Math.Sqrt(dx * dx + dy * dy);

            if (dist < this.chase_speed) return;

            double new_x = this.x + this.chase_speed * dx / dist;
            double new_y = this.y + this.chase_speed * dy / dist;

            double dist_from_center = Math.Sqrt(Math.Pow(new_x - this.start_x, 2) + Math.Pow(new_y - this.start_y, 2));
            if (dist_from_center <= this.zone_radius)
            {
                this.x = (int)new_x;
                this.y = (int)new_y;
            }
        }

        private bool is_in_zone(Agent a)
        {
            double dist = Math.Sqrt(Math.Pow(a.x - this.start_x, 2) + Math.Pow(a.y - this.start_y, 2));
            return dist <= this.zone_radius;
        }

        public void draw_zone(PaintEventArgs e)
        {
            if (this.start_x + Globals.window_slide < -this.zone_radius) return;

            Pen pen = new Pen(this.colour, 2);
            e.Graphics.DrawEllipse(
                pen,
                this.start_x - this.zone_radius + Globals.window_slide,
                this.start_y - this.zone_radius,
                2 * this.zone_radius,
                2 * this.zone_radius
            );
        }
    }
}
