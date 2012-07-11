using System;

namespace AAAA
{
	public static class StringExtensions
	{
		#region public static bool TryConvertTo(this string value, TypeCode typeCode, out object result)
		public static bool TryConvertTo(this string value, TypeCode typeCode, out object result)
		{
			if ((value == null) && (((typeCode == TypeCode.Empty) || (typeCode == TypeCode.String)) || (typeCode == TypeCode.Object)))
			{
				result = null;
				return true;
			}

			switch (typeCode)
			{
				case TypeCode.Empty:
					throw new InvalidCastException("Cannot convert to Empty");

				case TypeCode.DBNull:
					throw new InvalidCastException("Cannot convert to DBNull");

				case TypeCode.String:
				case TypeCode.Object:
					result = value;
					return true;

				case TypeCode.Boolean: return TryParseBool(value, out result);
				case TypeCode.Char: return TryParseChar(value, out result);
				case TypeCode.SByte: return TryParseSByte(value, out result);
				case TypeCode.Byte: return TryParseByte(value, out result);
				case TypeCode.Int16: return TryParseInt16(value, out result);
				case TypeCode.UInt16: return TryParseUInt16(value, out result);
				case TypeCode.Int32: return TryParseInt32(value, out result);
				case TypeCode.UInt32: return TryParseUInt32(value, out result);
				case TypeCode.Int64: return TryParseInt64(value, out result);
				case TypeCode.UInt64: return TryParseUInt64(value, out result);
				case TypeCode.Single: return TryParseSingle(value, out result);
				case TypeCode.Double: return TryParseDouble(value, out result);
				case TypeCode.Decimal: return TryParseDecimal(value, out result);
				case TypeCode.DateTime: return TryParseDateTime(value, out result);
			}
			throw new ArgumentException("Unknown TypeCode");
		}
		private static bool TryParseBool(string value, out object result)
		{
			bool r;
			bool success = bool.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseChar(string value, out object result)
		{
			char r;
			bool success = char.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseSByte(string value, out object result)
		{
			SByte r;
			bool success = SByte.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseByte(string value, out object result)
		{
			Byte r;
			bool success = Byte.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseInt16(string value, out object result)
		{
			Int16 r;
			bool success = Int16.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseUInt16(string value, out object result)
		{
			UInt16 r;
			bool success = UInt16.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseInt32(string value, out object result)
		{
			Int32 r;
			bool success = Int32.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseUInt32(string value, out object result)
		{
			UInt32 r;
			bool success = UInt32.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseInt64(string value, out object result)
		{
			Int64 r;
			bool success = Int64.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseUInt64(string value, out object result)
		{
			UInt64 r;
			bool success = UInt64.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseSingle(string value, out object result)
		{
			Single r;
			bool success = Single.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseDouble(string value, out object result)
		{
			Double r;
			bool success = Double.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseDecimal(string value, out object result)
		{
			Decimal r;
			bool success = Decimal.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		private static bool TryParseDateTime(string value, out object result)
		{
			DateTime r;
			bool success = DateTime.TryParse(value, out r);
			result = success ? (object)r : null;
			return success;
		}
		#endregion
	}
}
