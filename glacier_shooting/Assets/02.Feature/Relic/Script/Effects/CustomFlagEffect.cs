using System;

[Serializable]
public class CustomFlagEffect : ItemEffect
{
    public string key;
    public float value = 1f;

    public override void Apply(ItemContext ctx)
    {
        // 프로젝트 규칙에 맞게 연결 (인벤토리/룰 매니저 등)
        // 예) ctx.inventory?.SetFlag(key, value);
        Log(ctx, $"Flag set: {key} = {value}");
    }

    public override void Remove(ItemContext ctx)
    {
        // 필요 시 원복/삭제
        // 예) ctx.inventory?.ClearFlag(key);
    }

    public override string Summary() => $"(legacy) Flag {key}={value}";
}
