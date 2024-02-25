#nullable enable
using Game;
using Game.FunctionsLibrary;
using Game.Systems;

public static class FunctionExtensions
{
    public static object? Invoke<T>(this T function) where T : IFunctionDefinition
    {
        foreach (VirtualMachine vm in VirtualMachine.All)
        {
            if (vm.ContainsSystem<FunctionSystem>())
            {
                InvokeFunctionRequest request = InvokeFunctionRequest.Create(function);
                vm.Broadcast(ref request);
                return request.Result;
            }
        }

        return null;
    }
}