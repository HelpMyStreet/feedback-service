﻿using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using System;

namespace FeedbackService.Repo.Extensions
{
    public static class SqlConnectionExtensions
    {
        public static void AddAzureToken(this SqlConnection connection)
        {
            if (connection.DataSource.Contains("database.windows.net"))
            {
                connection.AccessToken = new AzureServiceTokenProvider().GetAccessTokenAsync("https://database.windows.net/").Result;
            }
        }
    }
}
