using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NauticalRenderer.Data;
using NauticalRenderer.Data.MapPack;
using NauticalRenderer.Graphics;
using NauticalRenderer.Nmea;
using NauticalRenderer.Utility;
using NmeaParser;
using NmeaParser.Messages;

namespace NauticalRenderer.SlippyMap.Layers
{
    class GpsLayer : MapLayer
    {
        /// <inheritdoc />
        public override ILayerSettings LayerSettings { get; }

        private NmeaDevice gpsDevice;

        private Vector2 currentCoords;
        private float currentRot;

        public GpsLayer()
        {
            GpsManager.GpsDataReceived += OnNmeaMessageReceived;
        } 

        /// <inheritdoc />
        public override void LoadContent(MapPack mapPack)
        {
            
        }

        /// <inheritdoc />
        public override void Draw(SpriteBatch sb, SpriteBatch mapSb, Camera camera)
        {
            sb.Draw(Icons.Vehicle, currentCoords.Transform(camera.GetMatrix()), null, Color.White, float.IsNaN(currentRot) ? 0 : currentRot, Icons.Vehicle.Bounds.Center.ToVector2(), 1.0f / 16, SpriteEffects.None, 0);
        }

        private void OnNmeaMessageReceived(object o, Rmc rmc)
        {
            currentCoords = new Vector2((float)rmc.Longitude, -(float)rmc.Latitude);
            currentRot = MathHelper.ToRadians((float)rmc.Course);
        }
    }
}
