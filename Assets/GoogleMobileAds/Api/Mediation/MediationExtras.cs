using System.Collections.Generic;

namespace GoogleMobileAds.Api.Mediation
{
    public abstract class MediationExtras
    {
        public MediationExtras()
        {
            Extras = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Extras { get; protected set; }

        public abstract string AndroidMediationExtraBuilderClassName { get; }

        public abstract string IOSMediationExtraBuilderClassName { get; }
    }
}