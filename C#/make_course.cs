using System;

namespace ObstacleCourse
{
    public static class MakeCourse
    {
        public static void intro_obstacles()
        {
            new Obstacle(
                    500,
                    Globals.top_border + (Globals.bottom_border - Globals.top_border) / 2,
                    100,
                    Color.Cyan
                );
        }
    }
}