using photoCon.Models;

namespace photoCon.Interface
{
    public interface IParametersRepository
    {
        Task<ICollection<TblParameter>> GetTblParametersAsync();
        Task<TblParameter> ParamDate(string parameterDescription);

    }
}
