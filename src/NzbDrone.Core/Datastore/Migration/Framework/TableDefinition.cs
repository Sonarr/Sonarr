// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator.Infrastructure.Extensions;
using FluentMigrator.Model;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public class TableDefinition : ICloneable
    {
        public TableDefinition()
        {
            Columns = new List<ColumnDefinition>();
            ForeignKeys = new List<ForeignKeyDefinition>();
            Indexes = new List<IndexDefinition>();
        }

        public virtual string Name { get; set; }
        public virtual string SchemaName { get; set; }
        public virtual ICollection<ColumnDefinition> Columns { get; set; }
        public virtual ICollection<ForeignKeyDefinition> ForeignKeys { get; set; }
        public virtual ICollection<IndexDefinition> Indexes { get; set; }

        public object Clone()
        {
            return new TableDefinition
            {
                Name = Name,
                SchemaName = SchemaName,
                Columns = Columns.CloneAll().ToList(),
                Indexes = Indexes.CloneAll().ToList()
            };
        }
    }
}
