// 둔화(이동속도 감소)를 받을 수 있는 대상.
// Enemy가 이 인터페이스를 구현하면 냉각체 장비의 둔화가 적용된다.
// (구현하기 전에는 둔화만 조용히 무시되고 나머지는 정상 작동)
public interface ISlowable
{
    // slowPercent: 이동속도 감소량(%). 20이면 20% 감소
    // duration: 지속 시간(초)
    void ApplySlow(float slowPercent, float duration);
}