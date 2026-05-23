using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Nursia.Utilities
{
	internal static class JsonExtensions
	{
		public static readonly JsonSerializerSettings DefaultOptions;

		private static float ParseFloat(string s)
		{
			return float.Parse(s.Trim(), CultureInfo.InvariantCulture);
		}

		private static int ParseInt(string s)
		{
			return int.Parse(s.Trim());
		}

		private class PointConverter : JsonConverter<Point>
		{
			public static readonly PointConverter Instance = new PointConverter();

			private PointConverter()
			{
			}

			public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				string s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Point(ParseInt(p[0]), ParseInt(p[1]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.X, value.Y);
				writer.WriteValue(str);
			}
		}

		private class ColorConverter : JsonConverter<Color>
		{
			public static readonly ColorConverter Instance = new ColorConverter();

			private ColorConverter()
			{
			}

			public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var result = ColorStorage.FromName(s);

				if (result == null)
				{
					throw new Exception($"Could not parse color {s}");
				}

				return result.Value;
			}

			public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
			{
				var str = value.GetColorName();

				if (str == null)
				{
					str = value.ToHexString();
				}

				writer.WriteValue(str);
			}
		}

		private class Vector2Converter : JsonConverter<Vector2>
		{
			public static readonly Vector2Converter Instance = new Vector2Converter();

			private Vector2Converter()
			{

			}

			public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Vector2(ParseFloat(p[0]), ParseFloat(p[1]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer options)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}", value.X, value.Y);
				writer.WriteValue(str);
			}
		}

		private class Vector3Converter : JsonConverter<Vector3>
		{
			public static readonly Vector3Converter Instance = new Vector3Converter();

			private Vector3Converter()
			{

			}

			public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Vector3(ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer options)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}", value.X, value.Y, value.Z);
				writer.WriteValue(str);
			}
		}

		private class Vector4Converter : JsonConverter<Vector4>
		{
			public static readonly Vector4Converter Instance = new Vector4Converter();

			private Vector4Converter()
			{

			}

			public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Vector4(ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer options)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.X, value.Y, value.Z, value.W);
				writer.WriteValue(str);
			}
		}

		private class QuaternionConverter : JsonConverter<Quaternion>
		{
			public static readonly QuaternionConverter Instance = new QuaternionConverter();

			private QuaternionConverter()
			{

			}

			public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Quaternion(ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer options)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.X, value.Y, value.Z, value.W);
				writer.WriteValue(str);
			}
		}

		private class PlaneConverter : JsonConverter<Plane>
		{
			public static readonly PlaneConverter Instance = new PlaneConverter();

			private PlaneConverter()
			{

			}

			public override Plane ReadJson(JsonReader reader, Type objectType, Plane existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				var p = s.Split(',');
				var result = new Plane(ParseFloat(p[0]), ParseFloat(p[1]), ParseFloat(p[2]), ParseFloat(p[3]));

				return result;
			}

			public override void WriteJson(JsonWriter writer, Plane value, JsonSerializer options)
			{
				var str = string.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", value.Normal.X, value.Normal.Y, value.Normal.Z, value.D);
				writer.WriteValue(str);
			}
		}

		private class StateConverter<T> : JsonConverter<T> where T : GraphicsResource
		{
			private readonly Dictionary<string, T> _values = new Dictionary<string, T>();

			public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
			{
				var s = reader.Value.ToString();

				T value;
				if (!_values.TryGetValue(s, out value))
				{
					throw new Exception($"Could not deserialize value {s} for type {typeof(T).Name}");
				}

				return value;
			}

			public override void WriteJson(JsonWriter writer, T value, JsonSerializer options)
			{
				writer.WriteValue(value.Name);
			}

			protected void AddValues(params T[] values)
			{
				foreach (var value in values)
				{
					_values[value.Name] = value;
				}
			}
		}

		private class BlendStateConverter : StateConverter<BlendState>
		{
			public static readonly BlendStateConverter Instance = new BlendStateConverter();

			private BlendStateConverter()
			{
				AddValues(BlendState.AlphaBlend, BlendState.NonPremultiplied, BlendState.Opaque, BlendState.Additive);
			}
		}

		private class DepthStencilStateConverter : StateConverter<DepthStencilState>
		{
			public static readonly DepthStencilStateConverter Instance = new DepthStencilStateConverter();

			private DepthStencilStateConverter()
			{
				AddValues(DepthStencilState.Default, DepthStencilState.DepthRead, DepthStencilState.None);
			}
		}

		private class RasterizerStateConverter : StateConverter<RasterizerState>
		{
			public static readonly RasterizerStateConverter Instance = new RasterizerStateConverter();

			private RasterizerStateConverter()
			{
				AddValues(RasterizerState.CullCounterClockwise, RasterizerState.CullClockwise, RasterizerState.CullNone);
			}
		}

		static JsonExtensions()
		{
			DefaultOptions = new JsonSerializerSettings
			{
				Culture = CultureInfo.InvariantCulture,
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.Auto,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				SerializationBinder = new CustomSerializationBinder()
			};

			DefaultOptions.Converters.Add(new StringEnumConverter());
			DefaultOptions.Converters.Add(PointConverter.Instance);
			DefaultOptions.Converters.Add(ColorConverter.Instance);
			DefaultOptions.Converters.Add(Vector2Converter.Instance);
			DefaultOptions.Converters.Add(Vector3Converter.Instance);
			DefaultOptions.Converters.Add(Vector4Converter.Instance);
			DefaultOptions.Converters.Add(QuaternionConverter.Instance);
			DefaultOptions.Converters.Add(PlaneConverter.Instance);
			DefaultOptions.Converters.Add(BlendStateConverter.Instance);
			DefaultOptions.Converters.Add(DepthStencilStateConverter.Instance);
			DefaultOptions.Converters.Add(RasterizerStateConverter.Instance);
		}

		public static string SerializeToString<T>(T data) => JsonConvert.SerializeObject(data, DefaultOptions);

		public static void SerializeToFile<T>(string path, T data)
		{
			File.WriteAllText(path, SerializeToString(data));
		}

		public static T DeserializeFromString<T>(string data)
		{
			return JsonConvert.DeserializeObject<T>(data, DefaultOptions);
		}

		public static T DeserializeFromFile<T>(string path)
		{
			var text = File.ReadAllText(path);
			return DeserializeFromString<T>(text);
		}
	}
}