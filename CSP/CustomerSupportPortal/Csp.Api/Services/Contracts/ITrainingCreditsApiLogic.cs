using Csp.Api.Model;

namespace Csp.Api.Services.Contracts
{
    public interface ITrainingCreditsApiLogic
    {
        CommonApiResponse<dynamic> GetTrainingCredits(long userAccountId, long supportAccountId);

        CommonApiResponse<dynamic> GetTrainingCourses(long userAccountId, long supportAccountId, string creditId);

        CommonApiResponse<dynamic> GetSelectedTrainingPartners(long userAccountId, long supportAccountId);

        CommonApiResponse<dynamic> GetTrainingPartners(long userAccountId, long supportAccountId);

        CommonApiResponse<dynamic> UpdateSelectedTrainingPartners(TrainingPartner trainingPartner);
    }
}
