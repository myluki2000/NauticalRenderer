using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;

namespace NauticalRenderer.Utility
{
    public static class Icons
    {
        public static Texture2D Vehicle { get; private set; }
        public static Texture2D Star { get; private set; }
        public static Texture2D LightBlob { get; private set; }
        public static Texture2D NoFishing { get; private set; }
        public static Texture2D RestrictedFishing { get; private set; }
        public static Texture2D WindFarm { get; private set; }
        public static Texture2D NoAnchoring { get; private set; }
        /// <summary>
        /// Texture containing all buoy shapes. Note that the coordinates for the different sprites in the atlas are
        /// defined in the BuoyEffect shader.
        /// </summary>
        public static Texture2D Buoys { get; set; }
        public static Texture2D Facilities { get; set; }

        public static class Landmarks
        {
            public static Texture2D Texture { get; private set; }

            public static Rectangle Chimney => new Rectangle(0, 0, 512, 512);
            public static Rectangle Cross => new Rectangle(512, 0, 512, 512);
            public static Rectangle DishAerial => new Rectangle(1024, 0, 512, 512);
            public static Rectangle Flagstaff => new Rectangle(1536, 0, 512, 512);
            public static Rectangle FlareStack => new Rectangle(0, 512, 512, 512);
            public static Rectangle Mast => new Rectangle(512, 512, 512, 512);
            public static Rectangle Tower => new Rectangle(1024, 512, 512, 512);
            public static Rectangle WindTurbine => new Rectangle(1536, 512, 512, 512);
            public static Rectangle Windmill => new Rectangle(0, 1024, 512, 512);
            public static Rectangle Windsock => new Rectangle(512, 1024, 512, 512);

            public static void LoadContent(ContentManager content)
            {
                Texture = content.Load<Texture2D>("Icons/landmarks");
            }
        }

        public static class Harbours
        {
            public static Texture2D Fishing { get; private set; }
            public static Texture2D Harbour { get; private set; }
            public static Texture2D Marina { get; private set; }
            public static Texture2D MarinaNoFacilities { get; private set; }

            public static void LoadContent(ContentManager content)
            {
                Fishing = content.Load<Texture2D>("Icons/Harbours/fishing_harbour");
                Harbour = content.Load<Texture2D>("Icons/Harbours/harbour");
                Marina = content.Load<Texture2D>("Icons/Harbours/marina");
                MarinaNoFacilities = content.Load<Texture2D>("Icons/Harbours/marina_no_facilities");
            }
        }

        public static void LoadContent(ContentManager content)
        {
            Vehicle = content.Load<Texture2D>("Icons/vehicle");
            Star = content.Load<Texture2D>("Icons/star");
            LightBlob = content.Load<Texture2D>("Icons/light_blob");
            NoFishing = content.Load<Texture2D>("Icons/no_fishing");
            RestrictedFishing = content.Load<Texture2D>("Icons/restricted_fishing");
            WindFarm = content.Load<Texture2D>("Icons/wind_farm");
            NoAnchoring = content.Load<Texture2D>("Icons/no_anchoring");
            Buoys = content.Load<Texture2D>("Icons/buoys");
            //Facilities = content.Load<Texture2D>("Icons/facilities");

            Landmarks.LoadContent(content);
            Harbours.LoadContent(content);
        }
    }
}
