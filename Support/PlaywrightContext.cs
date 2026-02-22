using Microsoft.Playwright;
using Reqnroll.BoDi;

namespace PlaywrightReqnRollCSharp.Support;

public class PlaywrightContext(IObjectContainer objectContainer)
{
    private readonly IObjectContainer _objectContainer = objectContainer;
    private IPage? _originalPageReference;
    private IPage? _currentPage;

    public IPage CurrentPage
    {
        get
        {
            _currentPage ??= _objectContainer.Resolve<IPage>();
            return _currentPage;
        }
        set
        {
            if (_originalPageReference == null && _currentPage != null)
            {
                _originalPageReference = _currentPage;
            }
            _currentPage = value;
        }
    }

    public IPage? InitialPage => _originalPageReference;

}