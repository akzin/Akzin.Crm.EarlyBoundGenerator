using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;

namespace Akzin.Crm.EarlyBoundGenerator.ObjectModel
{
    class Model
    {
        private readonly EntityMetadata entityMetadata;

        public Model(EntityMetadata entityMetadata, string ns)
        {
            this.entityMetadata = entityMetadata;
            Namespace = ns;
        }

        public string Namespace { get; }
        public string LogicalName => entityMetadata.LogicalName;

        public IEnumerable<Attribute> Attributes
        {
            get
            {
                var q = from a in entityMetadata.Attributes
                        where a.AttributeOf == null
                        where a.LogicalName != entityMetadata.PrimaryIdAttribute
                        where a.LogicalName != entityMetadata.LogicalName
                        orderby a.LogicalName
                        select new Attribute(a);
                return q;
            }
        }

        public Attribute Id
        {
            get
            {
                var a = entityMetadata.Attributes.First(x => x.LogicalName == entityMetadata.PrimaryIdAttribute);
                return new Attribute(a);
            }
        }

        public Attribute Name
        {
            get
            {
                var a = entityMetadata.Attributes.Where(x => x.LogicalName == entityMetadata.PrimaryNameAttribute).Select(x => new Attribute(x)).FirstOrDefault();
                return a;
            }
        }

        public Attribute StateAttribute
        {
            get
            {
                var q = from a in entityMetadata.Attributes
                        where a.AttributeType == AttributeTypeCode.State
                        select new Attribute(a);
                return q.First();
            }
        }

        public Attribute StatusAttribute
        {
            get
            {
                var q = from a in entityMetadata.Attributes
                        where a.AttributeType == AttributeTypeCode.Status
                        select new Attribute(a);
                return q.First();
            }
        }

        public string DisplayName
        {
            get
            {
                var displayName = entityMetadata
                    .DisplayName
                    .LocalizedLabels
                    .OrderBy(x => x.LanguageCode == 1043 ? 1 : 2) // give priority to dutch labels. 1043 == Dutch
                    .Select(x => x.Label).FirstOrDefault();
                return displayName ?? LogicalName;
            }
        }
    }
}