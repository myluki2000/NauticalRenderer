using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static NauticalRenderer.Globals;

namespace NauticalRenderer.Resources
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

            public static Rectangle Chimney => TEXINDEX_0_0;
            public static Rectangle Cross => TEXINDEX_1_0;
            public static Rectangle DishAerial => TEXINDEX_2_0;
            public static Rectangle Flagstaff => TEXINDEX_3_0;
            public static Rectangle FlareStack => TEXINDEX_0_1;
            public static Rectangle Mast => TEXINDEX_1_1;
            public static Rectangle Tower => TEXINDEX_2_1;
            public static Rectangle WindTurbine => TEXINDEX_3_1;
            public static Rectangle Windmill => TEXINDEX_0_2;
            public static Rectangle Windsock => TEXINDEX_1_2;

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

        public static class DangerSymbols
        {
            public static Texture2D Texture;

            public static Rectangle Rock => TEXINDEX_0_0;
            public static Rectangle RockAwash => TEXINDEX_1_0;
            public static Rectangle RockCovers => TEXINDEX_2_0;
            public static Rectangle Wreck => TEXINDEX_0_1;
            public static Rectangle WreckDangerous => TEXINDEX_1_1;
            public static Rectangle WreckShowing => TEXINDEX_2_1;

            public static void LoadContent(ContentManager content)
            {
                Texture = content.Load<Texture2D>("Icons/danger_symbols");
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
            Facilities = content.Load<Texture2D>("Icons/facilities");

            Landmarks.LoadContent(content);
            Harbours.LoadContent(content);
            DangerSymbols.LoadContent(content);
        }
    }
}
