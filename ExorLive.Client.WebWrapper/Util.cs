using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ExorLive
{
	public class Util
	{
		public static string ConvertToAcceptLanguageHeaderValue(List<CultureInfo> cultures)
		{
			var cultureStrings = new List<string>();
			foreach (var item in cultures.Distinct())
			{
				var name = item.Name;
				cultureStrings.Add(name);
				var shortname = item.TwoLetterISOLanguageName;
				cultureStrings.Add(shortname);
			}
			var listWithQualityValues = new List<string>(cultureStrings.Count);
			var pri = 1.0M;
			foreach (var item in cultureStrings)
			{
				if (pri <= 0.1M)
				{
					break;
				}
				else if (pri == 1.0M)
				{
					listWithQualityValues.Add(item);
				}
				else
				{
					listWithQualityValues.Add($"{item};q={pri.ToString(CultureInfo.InvariantCulture)}");
				}
				pri -= 0.1M;
			}
			listWithQualityValues.Add($"*;q=0.1");
			return string.Join(", ", listWithQualityValues);
		}

		/// <summary>
		/// Encodes a string to be represented as a string literal. The format
		/// is essentially a JSON string.
		/// </summary>
		/// <param name="str">The string.</param>
		/// <returns></returns>
		/// <remarks>
		/// http://weblog.west-wind.com/posts/2007/Jul/14/Embedding-JavaScript-Strings-from-an-ASPNET-Page
		/// </remarks>
		public static string EncodeJsString(string str)
		{
			var sb = new StringBuilder();
			foreach (var c in str)
			{
				switch (c)
				{
					case '\"':
						sb.Append("\\\"");
						break;
					case '\\':
						sb.Append("\\\\");
						break;
					case '\b':
						sb.Append("\\b");
						break;
					case '\f':
						sb.Append("\\f");
						break;
					case '\n':
						sb.Append("\\n");
						break;
					case '\r':
						sb.Append("\\r");
						break;
					case '\t':
						sb.Append("\\t");
						break;
					default:
						var i = (int)c;
						if (i < 32 || i > 127)
						{
							sb.AppendFormat("\\u{0:X04}", i);
						}
						else
						{
							sb.Append(c);
						}
						break;
				}
			}
			return sb.ToString();
		}
	}
}
