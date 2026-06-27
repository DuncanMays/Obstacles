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

        //public static void generate_obstacles(int x_disp)
        //{
        //    new Obstacle(
        //        x_disp,
        //        Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
        //        100,
        //        Color.Cyan
        //    );
        //}
    }
}
