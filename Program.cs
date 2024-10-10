using System;
using System.IO;
using System.Diagnostics;

namespace ProjectSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Welcome to the Project Setup Tool ");
            Console.WriteLine("Please choose the type of project you want to create: ");
            Console.Write("1- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("JavaScript Project");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("2- ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("TypeScript Project");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Enter your choice (1 or 2): ");

            string choice = Console.ReadLine().Trim();
            while (choice != "1" && choice != "2")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice. Please enter 1 for JavaScript or 2 for TypeScript. ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Enter your choice (1 or 2): ");
                choice = Console.ReadLine().Trim();
            }
            try
            {
                if (choice == "1")
                {
                    SetupJavaScriptProject();
                }
                else if (choice == "2")
                {
                    SetupTypeScriptProject();
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Project setup complete. ");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An error occurred during project setup: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ex.Message);
            }
        }
        static void SetupJavaScriptProject()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            Console.Write("Enter Project Name: ");
            string projectName = Console.ReadLine().Trim();
            string projectPath = Path.Combine(desktopPath, projectName);
            Directory.CreateDirectory(projectPath);
            Directory.SetCurrentDirectory(projectPath);
            Directory.CreateDirectory("controllers");
            Directory.CreateDirectory("models");
            Directory.CreateDirectory("routes");
            Directory.CreateDirectory("middleware");
            Directory.CreateDirectory("db");
            File.WriteAllText(".env", "PORT=5000\nMONGO_URI=");
            string gitignoreContent = "/node_modules\n/dist\n/.env\n";
            File.WriteAllText(".gitignore", gitignoreContent);
            File.WriteAllText("app.js", "");
            string connectJsContent = @"const mongoose = require('mongoose')

const connectDB = (url) => {
  return mongoose.connect(url)
}

module.exports = connectDB
";
            File.WriteAllText(Path.Combine("db", "connect.js"), connectJsContent);

            Console.Write("Enter the name for your route file: ");
            string routeFileName = GetValidInput("Route file name cannot be empty. Please enter a valid name: ");
            string routeFileLower = routeFileName.Replace(" ", "").Replace("\"", "");
            string routeFile = Path.Combine("routes", $"{routeFileLower}Routes.js");
            Console.Write("Enter the name for your controller file: ");
            string controllerFileName = GetValidInput("Controller file name cannot be empty. Please enter a valid name: ");
            string controllerFileLower = controllerFileName.Replace(" ", "").Replace("\"", "");
            string controllerFile = Path.Combine("controllers", $"{controllerFileLower}Controllers.js");
            Console.Write("Enter the name for your model file: ");
            string modelFileName = GetValidInput("Model file name cannot be empty. Please enter a valid name: ");
            string modelFileLower = modelFileName.Replace(" ", "").Replace("\"", "");
            string modelFile = Path.Combine("models", $"{modelFileLower}.js");
            string routeContent = $@"const express = require('express')
const {{
}} = require('../controllers/{controllerFileLower}')
const router = express.Router()

router.route('/')
router.route('/:id')

module.exports = router
";
            File.WriteAllText(routeFile, routeContent);
            File.WriteAllText(controllerFile, "");

            Console.Write("Enter the port number your server will listen on [default is 5000]: ");
            string portNumber = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(portNumber))
            {
                portNumber = "5000";
            }
            else
            {
                while (!int.TryParse(portNumber, out _))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Invalid port number. Please enter a valid number: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    portNumber = Console.ReadLine().Trim();
                }
            }
            File.WriteAllText(".env", $"PORT={portNumber}\n");
            Console.Write("Enter your MongoDB connection string: ");
            string dbConnectionString = GetValidInput("Connection string cannot be empty. Please enter a valid MongoDB connection string: ");
            File.AppendAllText(".env", $"MONGO_URI={dbConnectionString}");

            string appJsContent = $@"const express = require('express');
const app = express();

const connectDB = require('./db/connect');
const dotenv = require('dotenv');
dotenv.config();

const port = process.env.PORT || {portNumber};

// Middleware and route setup can go here

const start = async () => {{
  try {{
    await connectDB(process.env.MONGO_URI);
    app.listen(port, () => console.log(`Server is listening on port ${{port}}`));
  }} catch (err) {{
    console.log(err);
  }}
}}

start();
";
            File.WriteAllText("app.js", appJsContent);

            Console.Write("Do you want the AsyncWrapper method to be added? (Y/N): ");
            string addAsyncWrapper = GetYesNoInput();

            if (addAsyncWrapper == "Y")
            {
                string asyncWrapperContent = @"const asyncWrapper = (fn) => {
  return async (req, res, next) => {
    try {
      await fn(req, res, next)
    } catch (error) {
      next(error)
    }
  }
}

module.exports = asyncWrapper
";
                File.WriteAllText(Path.Combine("middleware", "async.js"), asyncWrapperContent);
            }

            string controllerContent = $"const Task = require('../models/{modelFileLower}');\n";
            if (addAsyncWrapper == "Y")
            {
                controllerContent += "const asyncWrapper = require('../middleware/async');\n";
            }
            File.AppendAllText(controllerFile, controllerContent);

            string modelContent = @"const mongoose = require('mongoose')

const TaskSchema = new mongoose.Schema({
  // Define your schema fields here
})
module.exports = mongoose.model('', TaskSchema)
";
            File.WriteAllText(modelFile, modelContent);

            RunCommand("npm", "init -y");
            RunCommand("npm", "install express mongoose");
            RunCommand("npm", "install dotenv");
            RunCommand("npm", "install nodemon npm-run-all --save-dev");

            string packageJsonContent = @"{
  ""name"": """ + projectName.ToLower() + @""",
  ""version"": ""1.0.0"",
  ""main"": ""app.js"",
  ""dependencies"": {},
  ""devDependencies"": {},
  ""scripts"": {
  }
}
";
            File.WriteAllText("package.json", packageJsonContent);

            UpdatePackageJson(new[]
            {
                "\"start\": \"node app.js\"",
                "\"dev\": \"nodemon app.js\""
            });

            Console.WriteLine("JavaScript project setup is complete.");
        }

        static void SetupTypeScriptProject()
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            Console.Write("Enter Project Name: ");
            string projectName = Console.ReadLine().Trim();

            string projectPath = Path.Combine(desktopPath, projectName);
            Directory.CreateDirectory(projectPath);
            Directory.SetCurrentDirectory(projectPath);

            Directory.CreateDirectory("src");
            Directory.CreateDirectory("dist");

            Directory.CreateDirectory(Path.Combine("src", "controllers"));
            Directory.CreateDirectory(Path.Combine("src", "models"));
            Directory.CreateDirectory(Path.Combine("src", "routes"));
            Directory.CreateDirectory(Path.Combine("src", "middleware"));
            Directory.CreateDirectory(Path.Combine("src", "db"));

            File.WriteAllText(".env", "PORT=5000\nMONGO_URI=");

            string gitignoreContent = "/node_modules\n/dist\n/.env\n";
            File.WriteAllText(".gitignore", gitignoreContent);

            string connectTsContent = @"import mongoose from 'mongoose';

const connectDB = (url: string) => {
  return mongoose.connect(url);
}

export default connectDB;
";
            File.WriteAllText(Path.Combine("src", "db", "connect.ts"), connectTsContent);

            Console.Write("Enter the name for your route file: ");
            string routeFileName = GetValidInput("Route file name cannot be empty. Please enter a valid name: ");

            string routeFileLower = routeFileName.Replace(" ", "").Replace("\"", "");
            string routeFile = Path.Combine("src", "routes", $"{routeFileLower}Routes.ts");

            Console.Write("Enter the name for your controller file: ");
            string controllerFileName = GetValidInput("Controller file name cannot be empty. Please enter a valid name: ");

            string controllerFileLower = controllerFileName.Replace(" ", "").Replace("\"", "");
            string controllerFile = Path.Combine("src", "controllers", $"{controllerFileLower}Controllers.ts");

            Console.Write("Enter the name for your model file: ");
            string modelFileName = GetValidInput("Model file name cannot be empty. Please enter a valid name: ");

            string modelFileLower = modelFileName.Replace(" ", "").Replace("\"", "");
            string modelFile = Path.Combine("src", "models", $"{modelFileLower}.ts");

            string routeContent = $@"import express from 'express';
import {{
}} from '../controllers/{controllerFileLower}';
const router = express.Router();

router.route('/')
router.route('/:id')

export default router;
";
            File.WriteAllText(routeFile, routeContent);

            File.WriteAllText(controllerFile, "");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Enter the port number your server will listen on [default is 5000]: ");
            Console.ForegroundColor = ConsoleColor.White;
            string portNumber = Console.ReadLine().Trim();
            if (string.IsNullOrEmpty(portNumber))
            {
                portNumber = "5000";
            }
            else
            {
                while (!int.TryParse(portNumber, out _))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("Invalid port number. Please enter a valid number: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    portNumber = Console.ReadLine().Trim();
                }
            }
            File.WriteAllText(".env", $"PORT={portNumber}\n");

            Console.Write("Enter your MongoDB connection string: ");
            string dbConnectionString = GetValidInput("Connection string cannot be empty. Please enter a valid MongoDB connection string: ");
            File.AppendAllText(".env", $"MONGO_URI={dbConnectionString}");

            string appTsContent = $@"import express from 'express';
import connectDB from './db/connect';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
const port = process.env.PORT || {portNumber};

// Middleware and route setup can go here

const start = async () => {{
  try {{
    await connectDB(process.env.MONGO_URI!);
    app.listen(port, () => console.log(`Server is listening on port ${{port}}`));
  }} catch (err) {{
    console.log(err);
  }}
}}

start();
";
            File.WriteAllText(Path.Combine("src", "app.ts"), appTsContent);

            Console.Write("Do you want the AsyncWrapper method to be added? (Y/N): ");
            string addAsyncWrapper = GetYesNoInput();

            if (addAsyncWrapper == "Y")
            {
                string asyncWrapperContent = @"import { Request, Response, NextFunction } from 'express';

const asyncWrapper = (fn: Function) => {
  return async (req: Request, res: Response, next: NextFunction) => {
    try {
      await fn(req, res, next);
    } catch (error) {
      next(error);
    }
  }
}

export default asyncWrapper;
";
                File.WriteAllText(Path.Combine("src", "middleware", "async.ts"), asyncWrapperContent);
            }

            string controllerContent = $"import Task from '../models/{modelFileLower}';\n";
            if (addAsyncWrapper == "Y")
            {
                controllerContent += "import asyncWrapper from '../middleware/async';\n";
            }
            File.AppendAllText(controllerFile, controllerContent);

            string modelContent = @"import mongoose, { Schema, Document } from 'mongoose';

interface ITask extends Document {
  // Define your schema interface here
}

const TaskSchema: Schema = new Schema({
  // Define your schema fields here
});

export default mongoose.model<ITask>('', TaskSchema);
";
            File.WriteAllText(modelFile, modelContent);

            RunCommand("npm", "init -y");
            RunCommand("npm", "install typescript --save-dev");
            RunCommand("npm", "install express mongoose");
            RunCommand("npm", "install dotenv");
            RunCommand("npm", "install nodemon npm-run-all --save-dev");
            RunCommand("npm", "install --save-dev @types/express @types/mongoose @types/node eslint eslint-config-prettier eslint-plugin-prettier prettier ts-node ts-node-dev");

            string packageJsonContent = @"{
  ""name"": """ + projectName.ToLower() + @""",
  ""version"": ""1.0.0"",
  ""main"": ""dist/app.js"",
  ""dependencies"": {},
  ""devDependencies"": {},
  ""scripts"": {
  }
}
";
            File.WriteAllText("package.json", packageJsonContent);

            string tsconfigContent = @"{
  ""compilerOptions"": {
    ""target"": ""ES6"",
    ""module"": ""commonjs"",
    ""rootDir"": ""./src"",
    ""outDir"": ""./dist"",
    ""strict"": true,
    ""esModuleInterop"": true,
    ""skipLibCheck"": true,
    ""forceConsistentCasingInFileNames"": true
  }
}
";
            File.WriteAllText("tsconfig.json", tsconfigContent);

            UpdatePackageJson(new[]
            {
                "\"start\": \"node dist/app.js\"",
                "\"dev\": \"npm-run-all --parallel watch:build watch:serve\"",
                "\"watch:build\": \"tsc --watch\"",
                "\"watch:serve\": \"nodemon dist/app.js\""
            });
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("TypeScript project setup is complete.");
            Console.ForegroundColor = ConsoleColor.White;
        }
        static string GetValidInput(string errorMessage)
        {
            string input = Console.ReadLine().Trim();
            while (string.IsNullOrEmpty(input))
            {
                Console.Write(errorMessage);
                input = Console.ReadLine().Trim();
            }
            return input;
        }
        static string GetYesNoInput()
        {
            string input = Console.ReadLine().Trim().ToUpper();
            while (input != "Y" && input != "N")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Invalid input. Please enter Y or N: ");
                Console.ForegroundColor = ConsoleColor.White;
                input = Console.ReadLine().Trim().ToUpper();
            }
            return input;
        }
        static void RunCommand(string command, string arguments)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = $"/c {command} {arguments}";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;
            psi.CreateNoWindow = false;
            psi.WorkingDirectory = Directory.GetCurrentDirectory();

            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                Console.WriteLine(output);
            }
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
            }
        }
        static void UpdatePackageJson(string[] scripts)
        {
            string packageJsonPath = Path.Combine(Directory.GetCurrentDirectory(), "package.json");
            if (File.Exists(packageJsonPath))
            {
                string packageJson = File.ReadAllText(packageJsonPath);

                int scriptsIndex = packageJson.IndexOf("\"scripts\": {");
                if (scriptsIndex != -1)
                {
                    int insertIndex = packageJson.IndexOf("{", scriptsIndex) + 1;
                    string newScripts = "\n    " + string.Join(",\n    ", scripts) + "\n  ";
                    int closeBraceIndex = packageJson.IndexOf("}", insertIndex);

                    packageJson = packageJson.Remove(insertIndex, closeBraceIndex - insertIndex);
                    packageJson = packageJson.Insert(insertIndex, newScripts);

                    File.WriteAllText(packageJsonPath, packageJson);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not find \"scripts\" section in package.json to update start script.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("package.json not found. Cannot update start script.");
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}