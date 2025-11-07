using System;
using System.Net;
using System.Threading.Tasks;

class Program
{

	static int CountMaskBits(uint mask)
	{
		int count = 0;
		for (int i = 0; i < 32; i++)
		{
			if ((mask & (1u << (31 - i))) != 0) count++;
		}
		return count;
	}
	static string ToIP(uint val)
	{
		return $"{(val >> 24) & 255}.{(val >> 16) & 255}.{(val >> 8) & 255}.{val & 255}";
	}
	static string GetIPClass(byte firstOctet)
	{
		if (firstOctet <= 127)
			return "A (1.0.0.0 – 126.255.255.255)";
		else if (firstOctet >= 128 && firstOctet <= 191)
			return "B (128.0.0.0 – 191.255.255.255)";
		else if (firstOctet >= 192 && firstOctet <= 223)
			return "C (192.0.0.0 – 223.255.255.255)";
		else if (firstOctet >= 224 && firstOctet <= 239)
			return "D (224.0.0.0 – 239.255.255.255) — Многоадресная";
		else
			return "E (240.0.0.0 – 255.255.255.255) — Экспериментальная";
	}
	
	static void Main()
	{
		uint mask;
		int prefix;
		string[] IP;
		byte okt1, okt2, okt3, okt4;

		while (true)
		{
		    Console.Write("Введите IP (четыре числа через точку): ");
		    IP = Console.ReadLine().Trim().Split('.');//вдуг введу пробел в начале или в конце,Trim() удалит, удобно однако

			if (IP.Length != 4 ||
				!byte.TryParse(IP[0], out okt1) ||
				!byte.TryParse(IP[1], out okt2) ||
				!byte.TryParse(IP[2], out okt3) ||
				!byte.TryParse(IP[3], out okt4))
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Неверный формат IP! Попробуйте снова.\n");
				Console.ResetColor();
				continue;
			}
			break;
		}

		okt1 = byte.Parse(IP[0]);
		okt2 = byte.Parse(IP[1]);
		okt3 = byte.Parse(IP[2]);
		okt4 = byte.Parse(IP[3]);

		while (true)
		{
			Console.Write("Введите маску (например 26(/26) или 255.255.255.192): ");
			string maskInput = Console.ReadLine().Trim();
			try
			{
				if (maskInput.Contains(".")) // маска в виде 255.255.255.??
				{
					string[] parts = maskInput.Split('.');
					if (parts.Length != 4) throw new Exception("Неверная маска");
					byte mokt1 = byte.Parse(parts[0]);
					byte mokt2 = byte.Parse(parts[1]);
					byte mokt3 = byte.Parse(parts[2]);
					byte mokt4 = byte.Parse(parts[3]);
					mask = (uint)((mokt1 << 24) | (mokt2 << 16) | (mokt3 << 8) | mokt4);
					prefix = CountMaskBits(mask);
				}
				else // формат /?? или просто число
				{
					if (maskInput.StartsWith("/"))
						maskInput = maskInput.Substring(1);
					prefix = int.Parse(maskInput);
					if (prefix < 0 || prefix > 32) throw new Exception("Неверный префикс");
					mask = prefix == 0 ? 0u : (uint.MaxValue << (32 - prefix));
				}
				break;
			}
			catch
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Неверная маска! Введите снова.\n");
				Console.ResetColor();
			}
		}

		uint ip = (uint)((okt1 << 24) | (okt2 << 16) | (okt3 << 8) | okt4);

		//возьму 192.168.1.5
		uint network = ip & mask;
		//из 11000000.10101000.00000001.00000101 -> 11111111.11111111.11111111.00000000 = 11000000.10101000.00000001.00000000
		//меняется последний октет(остается только сетевая часть)
		uint broadcast = network | ~mask;
		//из маски 11111111.11111111.11111111.00000000 -> инвертируем -> 00000000.00000000.00000000.11111111
		//теперь для бродкаста меняем последний октат у сетевого адреса -> 11000000.10101000.00000001.11111111 

		uint total = (uint)(1u << (32 - prefix));
		uint hosts = (prefix >= 31) ? 0u : total - 2u;

		string ipClass = GetIPClass(okt1);

		Console.WriteLine($"\nРезультаты:");
		Console.WriteLine($"IP:                {ToIP(ip)}");
		Console.WriteLine($"Класс IP:          {ipClass}");
		Console.WriteLine($"Маска:             {ToIP(mask)}");
		Console.WriteLine($"Префикс:           /{prefix}");
		Console.WriteLine($"Адрес сети:        {ToIP(network)}");
		Console.WriteLine($"Широковещательный адрес: {ToIP(broadcast)}");
		Console.WriteLine($"Всего адресов:     {total}");
		Console.WriteLine($"Доступные хосты:  {hosts}");
	}
}