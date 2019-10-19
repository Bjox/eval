namespace Eval.Core.Util.Roulette
{
    public interface IRoulette<T>
    {
        T Spin();
        T SpinAndRemove();
    }
}
