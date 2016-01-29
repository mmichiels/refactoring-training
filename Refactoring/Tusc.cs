using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refactoring
{
    public class Tusc
    {
        public static void Start(List<User> users, List<Product> products)
        {

            int totalNumberAppUsers = users.Count;
            int totalNumberProducts = products.Count;

            WriteWelcomeMessage();

            // Login
            Login:
            bool userIsLoggedIn = false; 

            var userName = PromptForUserName();
            var userPassword = string.Empty;

            if (string.IsNullOrEmpty(userName))
            {
                CloseConsoleIfEnterKeyPressed();
            }
            else
            {
                bool usernameIsValid = UsernameIsValid(users, userName, totalNumberAppUsers);
                if (usernameIsValid)
                {
                    userPassword = PromptForPassword();

                    bool userPasswordIsValid = UserPasswordIsValid(users, totalNumberAppUsers, userName, userPassword);

                    if (userPasswordIsValid)
                    {
                        userIsLoggedIn = true;
                    }
                    else
                    {
                        DisplayErrorToUser("You entered an invalid password.");

                        goto Login;
                    }
                }
                else
                {
                    DisplayErrorToUser("You entered an invalid user.");

                    goto Login;

                }
            }

            if (userIsLoggedIn)
            {
                ShowUserWelcomeMessage(userName);

                var userRemainingBalance = ShowUserRemainingBalance(users, totalNumberAppUsers, userName, userPassword);

                // Show product list
                while (true)
                {
                    PromptUserForPurchases(products, totalNumberProducts);

                    // Prompt for user input
                    Console.WriteLine("Enter a number:");
                    string productNumberAsString = Console.ReadLine();
                    int productNumber = Convert.ToInt32(productNumberAsString);
                    productNumber = productNumber - 1; // Subtract 1 from number

                    // Check if user entered number that equals product count
                    if (productNumber == totalNumberProducts)
                    {
                        var user = GetUserByUsernameAndPassword(userName, userPassword, users);
                        if (user != null)
                        {
                            user.Balance = userRemainingBalance;                            
                        }

                        WriteUserBalance(users);
                        WriteProductQuantities(products);
                        CloseConsoleIfEnterKeyPressed();

                        return;
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine("You want to buy: " + products[productNumber].Name);
                        Console.WriteLine("Your balance is " + userRemainingBalance.ToString("C"));

                        // Prompt for user input
                        Console.WriteLine("Enter amount to purchase:");
                        string amountToPurchaseAsString = Console.ReadLine();
                        int amountToPurchase = Convert.ToInt32(amountToPurchaseAsString);

                        // Check if balance - quantity * price is less than 0
                        if (userRemainingBalance - products[productNumber].Price * amountToPurchase < 0)
                        {
                            DisplayErrorToUser("You do not have enough money to buy that.");
                            continue;
                        }

                        // Check if quantity is less than quantity
                        if (products[productNumber].Quantity <= amountToPurchase)
                        {
                            DisplayErrorToUser("Sorry, " + products[productNumber].Name + " is out of stock");
                            continue;
                        }

                        // Check if quantity is greater than zero
                        if (amountToPurchase > 0)
                        {
                            // Balance = Balance - Price * Quantity
                            userRemainingBalance = userRemainingBalance - products[productNumber].Price*amountToPurchase;

                            // Quanity = Quantity - Quantity
                            products[productNumber].Quantity = products[productNumber].Quantity - amountToPurchase;

                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("You bought " + amountToPurchase + " " + products[productNumber].Name);
                            Console.WriteLine("Your new balance is " + userRemainingBalance.ToString("C"));
                            Console.ResetColor();
                        }
                        else
                        {
                            // Quantity is less than zero
                            Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine();
                            Console.WriteLine("Purchase cancelled");
                            Console.ResetColor();
                        }
                    }

                }
            }
        }

        private static void WriteWelcomeMessage()
        {
            Console.WriteLine("Welcome to TUSC");
            Console.WriteLine("---------------");
        }

        private static string PromptForUserName()
        {
            Console.WriteLine();
            Console.WriteLine("Enter Username:");
            string userName = Console.ReadLine();
            return userName;
        }

        private static bool UsernameIsValid(List<User> users, string userName, int totalNumberAppUsers)
        {
            for (int userIndex = 0; userIndex < totalNumberAppUsers; userIndex++)
            {
                User user = users[userIndex];
                if (user.Username == userName)
                {
                    return true;
                }
            }

            return false;
        }

        private static string PromptForPassword()
        {
            Console.WriteLine("Enter Password:");
            string pwd = Console.ReadLine();
            return pwd;
        }

        private static bool UserPasswordIsValid(List<User> users, int totalNumberAppUsers, string userName, string userPassword)
        {
            var user = GetUserByUsernameAndPassword(userName, userPassword, users);
            if (user != null)
            {
                    return true;
            }
            return false;
        }

        private static void ShowUserWelcomeMessage(string userName)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Login successful! Welcome " + userName + "!");
            Console.ResetColor();
        }

        private static double ShowUserRemainingBalance(List<User> users, int totalNumberAppUsers, string userName, string userPassword)
        {
            double userRemainingBalance = 0;
            var user = GetUserByUsernameAndPassword(userName, userPassword, users);

            if (user != null)
            {
                userRemainingBalance = user.Balance;
                Console.WriteLine();
                Console.WriteLine("Your balance is " + user.Balance.ToString("C"));
            }

            return userRemainingBalance;
        }

        private static void WriteProductQuantities(List<Product> products)
        {
            string json2 = JsonConvert.SerializeObject(products, Formatting.Indented);
            File.WriteAllText(@"Data/Products.json", json2);
        }

        private static void WriteUserBalance(List<User> users)
        {
            string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            File.WriteAllText(@"Data/Users.json", json);
        }

        private static void PromptUserForPurchases(List<Product> products, int totalNumberProducts)
        {
            Console.WriteLine();
            Console.WriteLine("What would you like to buy?");
            WriteProductsAndPrice(products, totalNumberProducts);
            Console.WriteLine(products.Count + 1 + ": Exit");
        }

        private static void WriteProductsAndPrice(List<Product> products, int totalNumberProducts)
        {
            for (int i = 0; i < totalNumberProducts; i++)
            {
                Product prod = products[i];
                Console.WriteLine(i + 1 + ": " + prod.Name + " (" + prod.Price.ToString("C") + ")");
            }
        }

        private static void DisplayErrorToUser(string error)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine(error);
            Console.ResetColor();
        }

        private static void CloseConsoleIfEnterKeyPressed()
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter key to exit");
            Console.ReadLine();
        }

        public static User GetUserByUsernameAndPassword(string userName, string userPassword, List<User> users)
        {
            int totalNumberAppUsers = users.Count;

            for (int userIndex = 0; userIndex < totalNumberAppUsers; userIndex++)
            {
                User user = users[userIndex];
                if (user.Username == userName && user.Password == userPassword)
                {
                    return user;
                }
            }

            return null;
        }
    }
}

