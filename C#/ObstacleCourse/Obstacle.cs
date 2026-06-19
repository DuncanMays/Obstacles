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
        Color colour = Obstacle.get_rnd_colour();

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
}
