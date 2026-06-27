namespace ObstacleCourse
{
    public static class MakeCourse
    {
        public static void intro_obstacles(int x_disp)
        {
            new Obstacle(
                x_disp,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                100,
                Color.Cyan
            );

            new Obstacle(
                x_disp + 200,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2 + 200,
                100,
                Color.Cyan
            );

            new Obstacle(
                x_disp + 200,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2 - 200,
                100,
                Color.Cyan
            );

            new Obstacle(
                x_disp + 400,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                100,
                Color.Cyan
            );

            new Obstacle(
                x_disp + 600,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2 + 200,
                100,
                Color.Cyan
            );

            new Obstacle(
                x_disp + 600,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2 - 200,
                100,
                Color.Cyan
            );

            new Slider(
                x_disp + 800,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                50,
                300,
                0,
                0.005,
                Color.Red
            );

            new Slider(
                x_disp + 950,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                50,
                300,
                0.5,
                0.0075,
                Color.Red
            );

            new Slider(
                x_disp + 1100,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                50,
                300,
                1,
                0.01,
                Color.Red
            );

            new Spinner(
                x_disp + 1500,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                70,
                200,
                0,
                0.007,
                Color.Yellow
            );

            new Spinner(
                x_disp + 1500,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                70,
                200,
                0.5 * Math.PI,
                0.007,
                Color.Yellow
            );

            new Spinner(
                x_disp + 1500,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                70,
                200,
                Math.PI,
                0.007,
                Color.Yellow
            );

            new Spinner(
                x_disp + 1500,
                Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                70,
                200,
                1.5 * Math.PI,
                0.007,
                Color.Yellow
            );

            // Grower gauntlet — alternating sides force agents to weave
            int mid = Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2;
            int grower_top = Globals.top_border + 150;
            int grower_bot = Globals.bottom_border - 150;
            double grower_speed = 0.008;

            for (int i = 0; i < 5; i++)
            {
                int gx = x_disp + 2000 + i * 250;
                double phase = i * Math.PI / 2;

                new Grower(gx, grower_top, 30, 160, phase, grower_speed, Color.Green);
                new Grower(gx, grower_bot, 30, 160, phase + Math.PI, grower_speed, Color.Green);
            }

            // Three trackers in quick succession
            int tracker_start = x_disp + 3500;
            for (int i = 0; i < 3; i++)
            {
                new Tracker(
                    tracker_start + i * 500,
                    mid,
                    40,
                    200,
                    2.5,
                    Color.Magenta
                );
            }
        }

        public static int generated_up_to = 0;
        static int chunk_width = 600;
        static double difficulty_scale = 2000.0;

        public static void generate_ahead(int frontier_x)
        {
            while (generated_up_to < frontier_x + 1500)
            {
                double difficulty = Math.Sqrt(generated_up_to / difficulty_scale);
                generate_chunk(generated_up_to, difficulty);
                generated_up_to += chunk_width;
            }
        }

        static void generate_chunk(int x, double difficulty)
        {
            Random rng = new Random(x);

            int height = Globals.bottom_border - Globals.top_border;
            int mid = Globals.top_border + height / 2;
            int margin = 80;

            double roll = rng.NextDouble();

            if (difficulty < 0.5 || roll < 0.3)
            {
                place_static_cluster(x, rng, height, margin, difficulty);
            }
            else if (roll < 0.55)
            {
                place_sliders(x, rng, mid, difficulty);
            }
            else if (roll < 0.7)
            {
                place_grower_pair(x, rng, height, difficulty);
            }
            else if (roll < 0.85)
            {
                place_spinner(x, rng, difficulty);
            }
            else
            {
                place_tracker(x, rng, difficulty);
            }
        }

        static void place_static_cluster(int x, Random rng, int height, int margin, double difficulty)
        {
            int count = 2 + (int)(difficulty * 2);
            count = Math.Min(count, 5);

            int safe_lane = rng.Next(Globals.top_border + margin, Globals.bottom_border - margin);

            for (int i = 0; i < count; i++)
            {
                int y = Globals.top_border + margin + rng.Next(0, height - 2 * margin);
                int rad = 40 + (int)(difficulty * 30);

                if (Math.Abs(y - safe_lane) < rad + 60) continue;

                new Obstacle(x + rng.Next(0, chunk_width), y, rad, Color.Cyan);
            }
        }

        static void place_sliders(int x, Random rng, int mid, double difficulty)
        {
            int count = 1 + (int)(difficulty * 0.8);
            count = Math.Min(count, 3);
            double speed = 0.004 + difficulty * 0.003;
            int bandwidth = 100 + (int)(difficulty * 80);
            int rad = 35 + (int)(difficulty * 15);

            for (int i = 0; i < count; i++)
            {
                double phase = rng.NextDouble() * 2 * Math.PI;
                int sx = x + (chunk_width / (count + 1)) * (i + 1);
                new Slider(sx, mid, rad, bandwidth, phase, speed, Color.Red);
            }
        }

        static void place_grower_pair(int x, Random rng, int height, double difficulty)
        {
            int grower_top = Globals.top_border + 150;
            int grower_bot = Globals.bottom_border - 150;
            double speed = 0.006 + difficulty * 0.004;
            int max_rad = 100 + (int)(difficulty * 60);
            max_rad = Math.Min(max_rad, height / 3);
            double phase = rng.NextDouble() * 2 * Math.PI;

            int gx = x + chunk_width / 2;
            new Grower(gx, grower_top, 20, max_rad, phase, speed, Color.Green);
            new Grower(gx, grower_bot, 20, max_rad, phase + Math.PI, speed, Color.Green);
        }

        static void place_spinner(int x, Random rng, double difficulty)
        {
            int arms = 2 + (int)(difficulty * 0.7);
            arms = Math.Min(arms, 4);
            int big_rad = 120 + (int)(difficulty * 40);
            int rad = 40 + (int)(difficulty * 20);
            rad = Math.Min(rad, 80);
            double speed = 0.005 + difficulty * 0.003;

            int sx = x + chunk_width / 2;
            int sy = Globals.top_border + 100 + rng.Next(0, Globals.bottom_border - Globals.top_border - 200);

            for (int i = 0; i < arms; i++)
            {
                double phase = i * 2 * Math.PI / arms;
                new Spinner(sx, sy, rad, big_rad, phase, speed, Color.Yellow);
            }
        }

        static void place_tracker(int x, Random rng, double difficulty)
        {
            int zone_rad = 150 + (int)(difficulty * 50);
            int kill_rad = 30 + (int)(difficulty * 10);
            kill_rad = Math.Min(kill_rad, 60);
            double chase_speed = 1.5 + difficulty * 1.0;

            int ty = Globals.top_border + zone_rad + rng.Next(0, Globals.bottom_border - Globals.top_border - 2 * zone_rad);
            new Tracker(x + chunk_width / 2, ty, kill_rad, zone_rad, chase_speed, Color.Magenta);
        }
    }
}
