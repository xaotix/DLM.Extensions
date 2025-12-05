using DLM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Conexoes
{
    public static class Extensoes_Abrir_Admin
    {
        static System.Security.SecureString ConvertToSecureString(string password)
        {
            var securePassword = new System.Security.SecureString();
            foreach (char c in password)
            {
                securePassword.AppendChar(c);
            }
            securePassword.MakeReadOnly();
            return securePassword;
        }
        public static void AbrirAsAdmin(this string appToRun, string arguments = "")
        {
            string domain = "medabil.com.br";
            string username = "med.admin";
            string usernameFull = $@"{domain}\{username}";
            //string password = "K@$p3rsk1@2023!@";
            string password = "B1td3f3nd3r@2025@";
            var workingDirectory = Environment.ExpandEnvironmentVariables(@"%windir%\system32");

            string filePath = Environment.ExpandEnvironmentVariables(@"%windir%\system32\cmd.exe");

            if (appToRun.Contem("%"))
            {
                appToRun = Environment.ExpandEnvironmentVariables(appToRun);
            }

            string fullCommand = $"/C start \"\" \"{appToRun}\" {arguments}";
            if (arguments == "" | arguments == null)
            {
                fullCommand = $"/C start \"\" \"{appToRun}\"";
            }
            try
            {


                var startInfo = new ProcessStartInfo
                {
                    UserName = username,
                    Password = ConvertToSecureString(password),
                    Domain = domain,
                    WorkingDirectory = workingDirectory,
                    FileName = filePath,
                    Arguments = fullCommand,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    LoadUserProfile = true,
                    CreateNoWindow = false
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                }
            }
            catch (Exception ex1)
            {
                ex1.Log();
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        Password = ConvertToSecureString("M&nfds14"),
                        UserName = "administrador",
                        WorkingDirectory = workingDirectory,
                        FileName = filePath,
                        Arguments = fullCommand,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        LoadUserProfile = true,
                        CreateNoWindow = false
                    };

                    using (var process = new Process { StartInfo = startInfo })
                    {
                        process.Start();
                    }
                }
                catch (Exception ex2)
                {
                    ex2.Log();
                }
            }
        }
    }
}
