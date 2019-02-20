namespace CreditCardApplications
{
    public class CreditCardApplicationEvaluator
    {
        private readonly IFrequentFlyerNumberValidator _validator;

        private const int AutoReferralMaxAge = 20;
        private const int HighIncomeThreshhold = 100_000;
        private const int LowIncomeThreshhold = 20_000;

        public CreditCardApplicationEvaluator(IFrequentFlyerNumberValidator validator)
        {
            _validator = validator ??
                throw new System.ArgumentNullException(nameof(validator));
        }

        public CreditCardApplicationDecision Evaluate(CreditCardApplication application)
        {
            if (application.GrossAnnualIncome >= HighIncomeThreshhold)
            {
                return CreditCardApplicationDecision.AutoAccepted;
            }

            if (_validator.ServiceInformation.License.LicenseKey == "EXPIRED")
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            _validator.ValidationMode = application.Age >= 30 ? ValidationMode.Detailed : 
                                                                ValidationMode.Quick;

            var isValidFrequentFlyerNumber =
                _validator.IsValid(application.FrequentFlyerNumber);

            if (!isValidFrequentFlyerNumber)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.Age <= AutoReferralMaxAge)
            {
                return CreditCardApplicationDecision.ReferredToHuman;
            }

            if (application.GrossAnnualIncome < LowIncomeThreshhold)
            {
                return CreditCardApplicationDecision.AutoDeclined;
            }

            return CreditCardApplicationDecision.ReferredToHuman;
        }

        //public CreditCardApplicationDecision EvaluateUsingOut(CreditCardApplication application)
        //{
        //    if (application.GrossAnnualIncome >= HighIncomeThreshhold)
        //    {
        //        return CreditCardApplicationDecision.AutoAccepted;
        //    }

        //    _validator.IsValid(application.FrequentFlyerNumber, out var isValidFrequentFlyerNumber);

        //    if (!isValidFrequentFlyerNumber)
        //    {
        //        return CreditCardApplicationDecision.ReferredToHuman;
        //    }

        //    if (application.Age <= AutoReferralMaxAge)
        //    {
        //        return CreditCardApplicationDecision.ReferredToHuman;
        //    }

        //    if (application.GrossAnnualIncome < LowIncomeThreshhold)
        //    {
        //        return CreditCardApplicationDecision.AutoDeclined;
        //    }

        //    return CreditCardApplicationDecision.ReferredToHuman;
        //}
    }
}
