using AspNetCore.Authentication.Internal.Events;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;

namespace AspNetCore.Authentication.Internal
{
    public class InternalOptions : AuthenticationSchemeOptions
    {
        private string _delimiter = ",";

        public InternalOptions()
        {
            ClaimMaps = new();
            Source = ClaimSource.HeaderAndQuery;
            AddSubjectMapping("subject");
            AddNameMapping("name");
            AddRoleMapping("role");
        }

        public new InternalEvents Events
        {
            get => (InternalEvents)base.Events;
            set => base.Events = value;
        }

        internal string SubjectClaimType { get; set; }
        internal string NameClaimType { get; set; }
        internal string RoleClaimType { get; set; }
        internal Dictionary<string, string> ClaimMaps { get; set; }
        public ClaimSource Source { get; set; }

        public string Delimiter
        {
            get => _delimiter;
            set => _delimiter = string.IsNullOrEmpty(value) ? "," : value;
        }

        public InternalOptions AddSubjectMapping(string fieldName)
            => AddSubjectMapping(fieldName, fieldName);

        public InternalOptions AddSubjectMapping(string fieldName, string claimType)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            if (string.IsNullOrWhiteSpace(claimType))
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            SubjectClaimType = claimType;
            ClaimMaps.Add(fieldName, claimType);
            return this;
        }

        public InternalOptions AddNameMapping(string fieldName)
            => AddNameMapping(fieldName, fieldName);

        public InternalOptions AddNameMapping(string fieldName, string claimType)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            if (string.IsNullOrWhiteSpace(claimType))
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            NameClaimType = claimType;
            ClaimMaps.Add(fieldName, claimType);
            return this;
        }

        public InternalOptions AddRoleMapping(string fieldName)
            => AddRoleMapping(fieldName, fieldName);

        public InternalOptions AddRoleMapping(string fieldName, string claimType)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            if (string.IsNullOrWhiteSpace(claimType))
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            RoleClaimType = claimType;
            ClaimMaps.Add(fieldName, claimType);
            return this;
        }

        public InternalOptions AddClaimMapping(string fieldName)
            => AddClaimMapping(fieldName, fieldName);

        public InternalOptions AddClaimMapping(string fieldName, string claimType)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentNullException(nameof(fieldName));
            }
            if (string.IsNullOrWhiteSpace(claimType))
            {
                throw new ArgumentNullException(nameof(claimType));
            }

            ClaimMaps.Add(fieldName, claimType);
            return this;
        }
    }
}