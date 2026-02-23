using DaJet.Data;
using DaJet.Data.PostgreSql;
using DaJet.Metadata.Core;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace DaJet.Metadata.PostgreSql
{
    public sealed class PgMetadataProvider : IMetadataProvider
    {
        private readonly string _connectionString;
        public PgMetadataProvider(in string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            _connectionString = connectionString;
        }
        public int YearOffset { get { return 0; } }
        public string ConnectionString { get { return _connectionString; } }
        public DatabaseProvider DatabaseProvider { get { return DatabaseProvider.PostgreSql; } }
        public IQueryExecutor CreateQueryExecutor() { return new PgQueryExecutor(_connectionString); }
        public IEnumerable<MetadataItem> GetMetadataItems(Guid type)
        {
            IQueryExecutor executor = QueryExecutor.Create(DatabaseProvider.PostgreSql, _connectionString);

            string script = SQLHelper.GetTableSelectScript();

            List<MetadataItem> list = new();

            foreach (IDataReader reader in executor.ExecuteReader(script, 10))
            {
                list.Add(new MetadataItem(Guid.Empty, Guid.Empty, reader.GetString(0)));
            }

            return list;
        }
        public bool TryGetEnumValue(in string identifier, out EnumValue value)
        {
            throw new NotImplementedException();
        }
        public bool TryGetExtendedInfo(Guid uuid, out MetadataItemEx info)
        {
            info = MetadataItemEx.Empty;
            return false;
        }
        public IDbConfigurator GetDbConfigurator()
        {
            return new PgDbConfigurator(this);
        }
        public MetadataItem GetMetadataItem(int typeCode)
        {
            throw new NotImplementedException();
        }
        public MetadataObject GetMetadataObject(Guid type, Guid uuid)
        {
            throw new NotImplementedException();
        }
        public MetadataObject GetMetadataObject(string metadataName)
        {
            PgDbConfigurator configurator = new(this);

            // 1) Regular database table (INFORMATION_SCHEMA.COLUMNS-based mapping)
            MetadataObject metadata = configurator.GetTableDefinition(in metadataName);

            if (metadata is not null)
            {
                return metadata;
            }

            // 2) SQL user-defined type (CREATE TYPE ... AS TABLE)
            try
            {
                return configurator.GetTypeDefinition(in metadataName);
            }
            catch
            {
                return null;
            }
        }
    }
}
