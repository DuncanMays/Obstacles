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
