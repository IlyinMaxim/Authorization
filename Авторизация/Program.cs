using System.Text.RegularExpressions;
using System.Text;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Authorization
{
	class Program
	{
		static string filePath = "users.txt";
		static string allowedChars = "0123456789,!./<>?;':-";
		static bool isLoggedIn = false;
		static string loggedInUser = "";

		static void Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("-----------------------------------"); // Заголовок
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("-_-  Авторизация пользователя  -_-");
			Console.ForegroundColor = ConsoleColor.Blue;
			Console.WriteLine("-----------------------------------\n");

			while (true)
			{
				Console.ForegroundColor = ConsoleColor.Magenta;
				Console.WriteLine("1) Вход");
				Console.WriteLine("2) Смена пароля");
				Console.WriteLine("3) Выход");
				Console.WriteLine("4) Создать нвого пользователя");
				Console.ResetColor();

				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write("\nВыберите пункт, который вам нужен: "); //  Запрос выбора действия

				string choice = Console.ReadLine();

				switch (choice)
				{
					case "1":
						Login();
						break;
					case "2":
						if (isLoggedIn)
						{
							ChangePassword(loggedInUser);
						}
						else
						{
							Console.WriteLine("Вы должны войти в систему, чтобы сменить пароль.");
							Console.ReadKey(); //чтобы сообщение об ошибке не пропало мгновенно
						}
						break;
					case "3":
						Environment.Exit(0);
						break;
					default:
						Console.WriteLine("Неверный выбор.");
						break;
					case "4":
						AddUser();
						break;

						static void AddUser()
						{
							Console.Write("Введите ID или имя пользователя: ");
							string userID = Console.ReadLine();
							Console.Write("Введите пароль: ");
							string password = GetPassword();

							if (password.Length != 10 || !IsPasswordValid(password))
							{
								Console.WriteLine("Пароль должен состоять из 10 символов и содержать только цифры и специальные символы: 0123456789,!./<>?;':-");
								return;
							}

							Console.Write("Введите ФИО: ");
							string fullName = Console.ReadLine();
							Console.Write("Введите дату рождения: ");
							string birthDate = Console.ReadLine();
							Console.Write("Введите место рождения: ");
							string birthPlace = Console.ReadLine();
							Console.Write("Введите номер телефона: ");
							string phoneNumber = Console.ReadLine();

							if (IsLoginTaken(userID))
							{
								Console.WriteLine("Пользователь с таким ID или именем уже существует.");
								Console.WriteLine();
								return;
							}

							// Добавление нового пользователя в файл
							using (StreamWriter sw = File.AppendText(filePath))
							{
								sw.WriteLine($"{userID}:{password}:{fullName}:{birthDate}:{birthPlace}:{phoneNumber}");
							}

							Console.WriteLine("Пользователь успешно добавлен.");
							Console.WriteLine();
						}

						static bool IsPasswordValid(string password)
						{
							string validCharacters = "0123456789,!./<>?;':-";
							foreach (char c in password)
							{
								if (!validCharacters.Contains(c))
								{
									return false;
								}
							}
							return true;
						}

				}
			}
		}

		static void Login()
		{
			Console.Write("Логин: ");
			string login = Console.ReadLine();
			Console.Write("Пароль: ");
			string password = GetPassword();

			if (VerifyCredentials(login, password))
			{
				Console.ForegroundColor = ConsoleColor.Green; // Зеленый для успеха
				Console.WriteLine("Вход выполнен успешно!\n");
				Console.ResetColor();
				isLoggedIn = true;
				loggedInUser = login;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red; // Красный для ошибки
				Console.WriteLine("Неверный логин или пароль.\n");
				Console.ResetColor();
			}
		}

		static string GetPassword()
		{
			StringBuilder passwordBuilder = new StringBuilder();
			while (true)
			{
				ConsoleKeyInfo key = Console.ReadKey(true); // true - скрывает вводимые символы
				if (key.Key == ConsoleKey.Enter)
				{
					Console.WriteLine(); // Переход на новую строку после ввода
					break;
				}
				else if (key.Key == ConsoleKey.Backspace)
				{
					if (passwordBuilder.Length > 0)
					{
						passwordBuilder.Remove(passwordBuilder.Length - 1, 1);
						Console.Write("\b \b"); // Удаляем звездочку с экрана
					}
				}
				else
				{
					passwordBuilder.Append(key.KeyChar);
					Console.Write("*"); // Выводим звездочку вместо символа
				}
			}
			return passwordBuilder.ToString();
		}

		static bool IsLoginTaken(string login)
		{
			if (!File.Exists(filePath))
			{
				return false; // Если файл не существует, логин не может быть занят
			}

			string[] lines = File.ReadAllLines(filePath);
			foreach (string line in lines)
			{
				string[] credentials = line.Split(':');
				if (credentials[0] == login)
				{
					return true; // Логин занят
				}
			}

			return false; // Логин свободен
		}

		static void ChangePassword(string login)
		{
			string[] lines = File.ReadAllLines(filePath);
			string storedHashedPassword = null;

			foreach (string line in lines)
			{
				string[] credentials = line.Split(':');
				if (credentials[0] == login)
				{
					storedHashedPassword = credentials[1];
					break;
				}
			}

			if (storedHashedPassword == null)
			{
				Console.WriteLine("Неверный логин."); //  (на случай ошибки, хотя сюда не должно попасть)
				return;
			}

			Console.Write("Старый пароль: ");
			string oldPassword = Console.ReadLine();

			if ((oldPassword) != storedHashedPassword)
			{
				Console.WriteLine("Неверный старый пароль.");
				return;
			}

			Console.Write("Новый пароль: ");
			string newPassword = Console.ReadLine();

			// Проверка на совпадение с датой рождения пользователя
			string birthDate = "";
			foreach (string line in lines)
			{
				string[] credentials = line.Split(':');
				if (credentials[0] == login)
				{
					birthDate = credentials[3];
					break;
				}
			}

			if (newPassword == birthDate)
			{
				Console.WriteLine("Пароль должен состоять из 10 символов (цифр + знаков препинания) и не совпадать с датой рождения пользователя.");
				Console.WriteLine();
				return;
			}

			if (!IsValidPassword(newPassword))
			{
				Console.WriteLine("Пароль должен состоять из 10 символов (цифр + знаков препинания) и не совпадать с датой рождения пользователя.");
				Console.WriteLine();
				return;
			}

			for (int i = 0; i < lines.Length; i++)
			{
				string[] credentials = lines[i].Split(':');
				if (credentials[0] == login)
				{
					lines[i] = $"{login}:{newPassword}:{credentials[2]}:{credentials[3]}:{credentials[4]}:{credentials[5]}";
					File.WriteAllLines(filePath, lines);
					Console.WriteLine("Пароль успешно изменен.");
					break;
				}
			}
		}

		static bool VerifyCredentials(string login, string password)
		{
			if (!File.Exists(filePath)) return false;

			string[] lines = File.ReadAllLines(filePath);
			foreach (string line in lines)
			{
				string[] credentials = line.Split(':');
				if (string.Equals(credentials[0], login) && string.Equals(credentials[1], password))
				{
					return true;
				}
			}

			return false;
		}

		static bool IsValidPassword(string password)
		{
			// Проверка длины пароля
			if (password.Length != 10)
			{
				return false;
			}

			// Проверка на допустимые символы
			if (!Regex.IsMatch(password, "^[0123456789,!./<>?;':-]+$"))
			{
				return false;
			}
			return true;
		}
	}
}