using MindTouch.Dream;
using Usivity.Entities.Types;

namespace Usivity.Services.Core.Logic {

    public interface IAvatarHelper {

        //--- Methods ---
        XUri GetGravatarUri(string email);
        XUri GetAvatarUri(Identity identity);
    }
}
