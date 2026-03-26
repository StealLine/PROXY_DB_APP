// <copyright file="IcreateDB.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace PROXY_TODB.DBInterfaces
{
    using Npgsql;

    public interface IcreateDB
    {
        public Task<(string, string)> Create();
    }
}
