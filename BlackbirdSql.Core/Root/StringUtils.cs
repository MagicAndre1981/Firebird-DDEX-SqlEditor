﻿// Microsoft.Data.Tools.Utilities, Version=16.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.Data.Tools.Schema.Common.StringUtils

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BlackbirdSql.Core.Properties;
using BlackbirdSql.Core.Providers;



namespace BlackbirdSql.Core;

internal static class StringUtils
{
	public static string StripCR(string str)
	{
		if (str == null)
		{
			ArgumentNullException ex = new("str");
			Diag.Dug(ex);
			throw ex;
		}

		return str.Replace("\r", string.Empty);
	}

	public static int MultiLineCompare(string x, string y, StringComparison mode)
	{
		return string.Compare(StripCR(x), StripCR(y), mode);
	}

	public static bool MultiLineEquals(string x, string y, StringComparison mode)
	{
		return string.Equals(StripCR(x), StripCR(y), mode);
	}

	public static bool EmptyOrSpace(string str)
	{
		if (str != null)
		{
			return 0 >= str.Trim().Length;
		}

		return true;
	}

	public static bool NotEmptyAfterTrim(string str)
	{
		return !EmptyOrSpace(str);
	}

	public static bool EqualValue(string str1, string str2)
	{
		if (str1 != null && str2 != null)
		{
			return string.Compare(str1, str2, StringComparison.Ordinal) == 0;
		}

		return str1 == str2;
	}

	public static string CommentOut(string str)
	{
		if (str != null)
		{
			return "/*" + str + "*/";
		}

		return str;
	}

	public static bool IsSqlVariable(string variable)
	{
		if (variable.Length > 3 && variable.StartsWith("$(", StringComparison.Ordinal))
		{
			return variable.EndsWith(")", StringComparison.Ordinal);
		}

		return false;
	}

	public static string ExtractNameOfVariable(string variable)
	{
		return variable[2..^1];
	}

	public static List<Tuple<string, int>> GetVariablesFromString(string value)
	{
		List<Tuple<string, int>> list = new List<Tuple<string, int>>();
		if (!string.IsNullOrEmpty(value))
		{
			int num = 0;
			while (num < value.Length)
			{
				int num2 = value.IndexOf("$(", num, StringComparison.CurrentCultureIgnoreCase);
				if (num2 < 0)
				{
					break;
				}

				int num3 = value.IndexOf(")", num2 + 2, StringComparison.CurrentCultureIgnoreCase);
				if (num3 < num2 + 2)
				{
					break;
				}

				list.Add(new Tuple<string, int>(value.Substring(num2 + 2, num3 - num2 - 2), num2));
				num = num3 + 1;
			}
		}

		return list;
	}

	public static bool TryToParseCommandLineArguments(string arguments, out IList<KeyValuePair<string, string>> parameters, out string errorMessage)
	{
		arguments += " ";
		parameters = new List<KeyValuePair<string, string>>();
		errorMessage = string.Empty;
		Match match = Regex.Match(arguments, "\\s+", RegexOptions.CultureInvariant);
		int num = 0;
		while (match.Success)
		{
			if (match.Index > num)
			{
				string text = arguments[num..match.Index];
				text = text.TrimStart();
				num = arguments.IndexOf(text, num, StringComparison.Ordinal);
				if (text.StartsWith("\"", StringComparison.Ordinal))
				{
					int num2 = arguments.IndexOf('"', num + 1);
					if (num2 == -1)
					{
						errorMessage = HashLog.FormatHashed(CultureInfo.CurrentCulture, Resources.ArgumentParsing_DoubleQuoteMissingError);
						return false;
					}

					num++;
					text = arguments[num..num2];
					num = num2 + 1;
				}
				else
				{
					num = match.Index + match.Length;
				}

				if (text.StartsWith("/", StringComparison.OrdinalIgnoreCase))
				{
					parameters.Add(new KeyValuePair<string, string>(text, string.Empty));
				}
				else if (parameters.Count != 0)
				{
					parameters[parameters.Count - 1] = new KeyValuePair<string, string>(parameters[parameters.Count - 1].Key, text);
				}
			}

			match = match.NextMatch();
		}

		return true;
	}

	public static string ConvertByteArrayToHexString(byte[] content)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (content != null)
		{
			stringBuilder.Append("0x");
			foreach (byte b in content)
			{
				stringBuilder.Append(string.Format(CultureInfo.InvariantCulture, "{0:X2}", b));
			}
		}

		return stringBuilder.ToString();
	}

	public static byte[] ConvertHexStringToByteArray(string hexString)
	{
		byte[] array = null;
		if (hexString.Contains("\\"))
		{
			hexString = hexString.Replace("\\", string.Empty);
		}

		if (hexString.Contains("\r"))
		{
			hexString = hexString.Replace("\r", string.Empty);
		}

		if (hexString.Contains("\n"))
		{
			hexString = hexString.Replace("\n", string.Empty);
		}

		if (hexString.Length % 2 == 0)
		{
			int num = 0;
			int num2 = hexString.Length / 2;
			if (hexString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
			{
				num2--;
				num = 2;
			}

			hexString = hexString.ToUpperInvariant();
			array = new byte[num2];
			int num3 = Convert.ToInt32('A');
			int num4 = Convert.ToInt32('0');
			char[] array2 = new char[2];
			for (int i = 0; i < array.Length; i++)
			{
				int num5 = i * 2 + num;
				array2[0] = hexString[num5];
				array2[1] = hexString[num5 + 1];
				for (int j = 0; j < 2; j++)
				{
					int num6 = Convert.ToInt32(array2[j]);
					if (num6 >= num4 && num6 < num4 + 10)
					{
						if (j == 0)
						{
							array[i] = (byte)(num6 - num4 << 4);
						}
						else
						{
							array[i] += (byte)(num6 - num4);
						}
					}
					else if (num6 >= num3 && num6 < num3 + 6)
					{
						if (j == 0)
						{
							array[i] = (byte)(10 + num6 - num3 << 4);
						}
						else
						{
							array[i] += (byte)(10 + num6 - num3);
						}
					}
				}
			}
		}

		return array;
	}

	public static Encoding GetDacEncoding()
	{
		return Encoding.UTF8;
	}

	public static byte[] GetRandomBytes(int count)
	{
		using RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[count];
		rNGCryptoServiceProvider.GetBytes(array);
		return array;
	}

	public static string GeneratePassword(string userName)
	{
		if (userName == null)
		{
			return GeneratePassword();
		}

		int num = 5;
		string[] array = userName.Split(new char[6] { ',', '.', '\t', '-', '_', '#' }, StringSplitOptions.RemoveEmptyEntries);
		while (num-- > 0)
		{
			string text = GeneratePassword();
			if (userName.Length >= 3 && text.Contains(userName))
			{
				continue;
			}

			bool flag = true;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2.Length >= 3 && text.Contains(text2))
				{
					flag = false;
					break;
				}
			}

			if (flag)
			{
				return text;
			}
		}

		return GeneratePassword();
	}

	public static string GeneratePassword()
	{
		byte[] randomBytes = GetRandomBytes(48);
		Random random = new Random(Environment.TickCount);
		byte[] array = "msFT7_&#$!~<"u8.ToArray();
		Array.Copy(array, 0, randomBytes, randomBytes.GetLength(0) / 2, array.GetLength(0));
		StringBuilder stringBuilder = new StringBuilder();
		List<char> list = new()
		{
			'\'',
			'-',
			'*',
			'/',
			'\\',
			'"',
			'[',
			']',
			')',
			'('
		};
		for (int i = 0; i < randomBytes.GetLength(0); i++)
		{
			if (randomBytes[i] == 0)
			{
				randomBytes[i]++;
			}

			char c = Convert.ToChar(randomBytes[i]);
			if (c < ' ' || c > '~' || list.Contains(c))
			{
				c = (char)(97 + random.Next(0, 28));
			}

			stringBuilder.Append(c);
		}

		return stringBuilder.ToString();
	}

	[DebuggerStepThrough]
	public static string StripOffBrackets(string name)
	{
		if (!string.IsNullOrEmpty(name) && name.Length > 2)
		{
			int num = (name.StartsWith("[", StringComparison.Ordinal) ? 1 : 0);
			int num2 = (name.EndsWith("]", StringComparison.Ordinal) ? 1 : 0);
			if (num > 0 || num2 > 0)
			{
				name = name[num..^num2];
			}
		}

		return name;
	}

	public static bool HasMsBuildDelimiters(string name)
	{
		if (!string.IsNullOrEmpty(name) && name.Length > 3)
		{
			int num = (name.StartsWith("$(", StringComparison.Ordinal) ? 2 : 0);
			int num2 = (name.EndsWith(")", StringComparison.Ordinal) ? 1 : 0);
			if (num > 0 || num2 > 0)
			{
				return true;
			}
		}

		return false;
	}

	[DebuggerStepThrough]
	public static string StripOffMsBuildDelimiters(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}

		if (name.Length > 3)
		{
			name = name.Trim();
			int num = (name.StartsWith("$(", StringComparison.Ordinal) ? 2 : 0);
			int num2 = (name.EndsWith(")", StringComparison.Ordinal) ? 1 : 0);
			if (num > 0 || num2 > 0)
			{
				name = name[num..^num2];
			}
		}

		return name.Trim();
	}

	public static string EnsureIsMsBuildDelimited(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return name;
		}

		name = name.Trim();
		bool flag = !name.StartsWith("$(", StringComparison.Ordinal);
		bool flag2 = !name.EndsWith(")", StringComparison.Ordinal);
		if (flag || flag2)
		{
			name = (flag ? "$(" : string.Empty) + name + (flag2 ? ")" : string.Empty);
		}

		return name;
	}

	public static string EscapeDatabaseName(string database)
	{
		if (string.IsNullOrEmpty(database))
		{
			return database;
		}

		return database.Replace("'", "''");
	}

	public static string EscapeSqlCmdVariable(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return string.Empty;
		}

		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in value)
		{
			if (c == '"' || c == ']')
			{
				stringBuilder.Append(c);
			}

			stringBuilder.Append(c);
		}

		return stringBuilder.ToString();
	}

	public static string SQLString(string value, bool unicode)
	{
		return SQLString(value, 0, value.Length, unicode);
	}

	public unsafe static string SQLString(string value, int start, int length, bool unicode)
	{
		bool flag = true;
		int num = 0;
		if (value.Length == 0)
		{
			return (unicode ? "N" : "") + "''";
		}

		fixed (char* ptr = value)
		{
			for (int i = start; i < start + length; i++)
			{
				if (ptr[i] == '\'')
				{
					num++;
					flag = false;
				}

				if (ptr[i] == '\\' && i < start + length - 2 && ptr[i + 1] == '\r' && ptr[i + 2] == '\n')
				{
					num += 3;
					flag = false;
				}

				num++;
			}
		}

		if (flag)
		{
			return (unicode ? "N" : "") + "'" + value.Substring(start, length) + "'";
		}

		string text = new string(' ', num + (unicode ? 3 : 2));
		int num2 = 0;
		fixed (char* ptr3 = value)
		{
			fixed (char* ptr2 = text)
			{
				if (unicode)
				{
					ptr2[num2++] = 'N';
				}

				ptr2[num2++] = '\'';
				for (int j = start; j < start + length; j++)
				{
					if (ptr3[j] == '\'')
					{
						ptr2[num2++] = '\'';
					}

					if (ptr3[j] == '\\' && j < start + length - 2 && ptr3[j + 1] == '\r' && ptr3[j + 2] == '\n')
					{
						ptr2[num2++] = '\\';
						ptr2[num2++] = '\\';
						ptr2[num2++] = '\r';
						ptr2[num2++] = '\n';
						ptr2[num2++] = '\r';
						ptr2[num2++] = '\n';
						j += 2;
					}
					else
					{
						ptr2[num2++] = ptr3[j];
					}
				}

				ptr2[num2] = '\'';
			}
		}

		return text;
	}
}