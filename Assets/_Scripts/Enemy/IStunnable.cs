// [팀원에게 요청할 Tower.cs 변경사항]
// TremorBoss(패턴1 "주먹 내려찍기", 패턴3 "비명")는 범위 내 타워를 찾아
// IStunnable을 구현한 컴포넌트에만 ApplyStun(stunDuration)을 호출합니다.
// 즉 Tower.cs가 아래처럼 바뀌기 전까지는 보스가 범위 표시만 하고 실제로
// 타워를 멈추지는 못합니다.
//
// 1) public class Tower : MonoBehaviour, IStunnable 로 선언 변경
// 2) 필드 추가: private float stunEndTime = 0f;
// 3) 인터페이스 구현:
//        public void ApplyStun(float duration)
//        {
//            stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
//        }
// 4) AttackRoutine 코루틴에서 공격 직전에 기절 여부 체크 후 스킵:
//        if (Time.time < stunEndTime)
//        {
//            // 공격하지 않고 다음 틱으로
//        }
//        else
//        {
//            Enemy target = FindTarget();
//            ...
//        }
// (선택) 기절 중 시각 표현이 필요하면 stunEndTime 값을 참고해 스프라이트 색을
// 바꾸는 등의 연출을 추가하면 됩니다. 이 부분은 애니메이션 작업과 함께 나중에
// 처리해도 됩니다.
public interface IStunnable
{
    void ApplyStun(float duration);
}