namespace Krono
{
    public interface IEmailAlert
    {
        Response Test();

        Response Send();
    }
}