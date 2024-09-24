using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Taimi.UndaDaSea_BlishHUD
{
    // Thank to Teh from the BlishHUD discord for this class 🥰
    public class SkyLake
    {
        private readonly float _waterSurface;
        private readonly float _waterBottom;
        private readonly List<Vector3> bounds;
        private readonly Vector3 center;
        private readonly float radius;
        private float _distance;
        private readonly string _name;
        private readonly int _map;

        public SkyLake(float waterSurface, float waterBottom, List<Vector3> bounds, int map, string name)
        {
            _waterSurface = waterSurface;
            _waterBottom = waterBottom;
            _map = map;
            _name = name;
            this.bounds = bounds;
            center = GetCenter();
            radius = GetRadius();
        }

        public string Name
        { 
            get { return _name; }
        }

        public int Map
        {
            get { return _map; }
        }

        public float WaterSurface
        {
            get { return _waterSurface; }
        }

        public float WaterBottom
        {
            get { return _waterBottom; } 
        }

        [JsonIgnore]
        public float Distance
        {
            get { return _distance; }
        }

        public List<Vector3> Bounds
        {
            get { return bounds; }
        }

        //so we don't have to perform the more expensive calcs all the time
        public bool IsNearby(Vector3 playerPos)
        {
            _distance = Vector3.Distance(playerPos, center);
            if (_distance > radius * 1.5) return false; // arbitrary distance

            return true;
        }

        //water check
        public bool IsInWater(Vector3 playerPos)
        {
            //if not within top/bottom, not in water
            if (playerPos.Z < _waterBottom || playerPos.Z > (_waterSurface + 10))
                return false;

            return IsInPolygon(playerPos);
        }

        //Point in Polygon algorithm
        private bool IsInPolygon(Vector3 playerPos)
        {
            var count = bounds.Count;
            bool isInside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                bool isYAboveFirstVertex = bounds[i].Y > playerPos.Y;
                bool isYAboveSecondVertex = bounds[j].Y > playerPos.Y;

                if (isYAboveFirstVertex != isYAboveSecondVertex)
                {
                    float intersectionX = bounds[i].X + (playerPos.Y - bounds[i].Y) / (bounds[j].Y - bounds[i].Y) * (bounds[j].X - bounds[i].X);
                    if (playerPos.X < intersectionX)
                    {
                        isInside = !isInside;
                    }
                }
            }

            return isInside;
        }

        //get center of polygon
        private Vector3 GetCenter()
        {
            Vector3 center = new Vector3(0, 0, 0);

            foreach (var bound in bounds)
            {
                center += bound;
            }

            center /= bounds.Count;
            return center;
        }

        //get radius of polygon
        private float GetRadius()
        {
            float radius = 0;
            foreach (var bound in bounds)
            {
                float distance = Vector3.Distance(bound, center);
                if (distance > radius)
                    radius = distance;
            }

            return radius;
        }
    }

    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3 result = default(Vector3);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch (reader.Value.ToString())
                    {
                        case "X":
                            result.X = (float)reader.ReadAsDouble().Value;
                            break;
                        case "Y":
                            result.Y = (float)reader.ReadAsDouble().Value;
                            break;
                        case "Z":
                            result.Z = (float)reader.ReadAsDouble().Value;
                            break;
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                    break;
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            if (serializer.TypeNameHandling != TypeNameHandling.None)
            {
                writer.WritePropertyName("$type");
                writer.WriteValue(string.Format("{0}, {1}", value.GetType().ToString(), value.GetType().Assembly.GetName().Name));
            }
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WritePropertyName("Z");
            writer.WriteValue(value.Z);
            writer.WriteEndObject();
        }
    }
}