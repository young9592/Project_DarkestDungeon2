using UnityEngine;

public class CTheCrossroadsUIHeroPlacement : MonoBehaviour
{
    #region Inspector
    [SerializeField] private CTheCrossroadsUIPartySlot[] _mountingSlot;
    [SerializeField] private GameObject _startImage;
    #endregion

    private int[] _equippedHeros = { -1, -1, -1, -1 };

    public void GameStartUI()
    {
        Debug.Log("게임 시작");
        // 게임시작 
    }
    public void SetHeroSlot(int heroIndex)
    {
        // 이미 배치한 영웅인지 확인
        for (int i = 0; i < _equippedHeros.Length; i++)
        {
            if (_equippedHeros[i] == heroIndex)
            {
                return;
            }
        }

        // 첫 번째 빈 슬롯 찾기
        for (int i = 0; i < _equippedHeros.Length; i++)
        {
            if (_equippedHeros[i] == -1)
            {
                _equippedHeros[i] = heroIndex;

                CTheCrossroadsUIPartySlot slot = FindPartySlot(i);

                if (slot != null)
                {
                    slot.SetPartyHero(heroIndex);
                }

                UpdateGameStartUI();
                return;
            }
        }
    }

    public void ResetHeroSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedHeros.Length)
        {
            return;
        }

        _equippedHeros[slotIndex] = -1;

        CTheCrossroadsUIPartySlot slot = FindPartySlot(slotIndex);

        if (slot != null)
        {
            slot.ClearPartyHero();
        }

        UpdateGameStartUI();
    }
    public void RandomHeroSlot()
    {
        // 기존 파티 배치 초기화
        for (int i = 0; i < _equippedHeros.Length; i++)
        {
            _equippedHeros[i] = -1;

            CTheCrossroadsUIPartySlot slot = FindPartySlot(i);

            if (slot != null)
            {
                slot.ClearPartyHero();
            }
        }

        // 영웅 인덱스 배열 생성
        int[] heroIndices = { 0, 1, 2, 3 };

        // 배열 순서 무작위로 섞기
        for (int i = 0; i < heroIndices.Length; i++)
        {
            int randomIndex = Random.Range(i, heroIndices.Length);

            int temp = heroIndices[i];
            heroIndices[i] = heroIndices[randomIndex];
            heroIndices[randomIndex] = temp;
        }

        // 섞인 순서대로 파티에 배치
        for (int i = 0; i < heroIndices.Length; i++)
        {
            SetHeroSlot(heroIndices[i]);
        }
    }

    private CTheCrossroadsUIPartySlot FindPartySlot(int slotIndex)
    {
        for (int i = 0; i < _mountingSlot.Length; i++)
        {
            if (_mountingSlot[i] != null &&
                _mountingSlot[i].SlotIndex == slotIndex)
            {
                return _mountingSlot[i];
            }
        }

        return null;
    }

    private void UpdateGameStartUI()
    {
        for (int i = 0; i < _equippedHeros.Length; i++)
        {
            if (_equippedHeros[i] == -1)
            {
                _startImage.SetActive(false);
                return;
            }
        }

        _startImage.SetActive(true);
    }

}