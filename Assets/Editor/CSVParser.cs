#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public class CSVParser : Editor
{
    [MenuItem("Tools/Parse Game CSV Data")]
    public static void ParseCSV()
    {
        string assetPath = "Assets/_Scripts/Scriptable/GameDataManager.asset";

        GameDataManager dataManager =
            AssetDatabase.LoadAssetAtPath<GameDataManager>(assetPath);

        if (dataManager == null)
        {
            Debug.LogError(
                $"[{assetPath}] 경로에서 GameDataManager 에셋을 찾을 수 없습니다. 에셋을 먼저 생성해주세요!"
            );

            return;
        }

        dataManager.monsterDataTable.Clear();
        dataManager.waveDataTable.Clear();
        dataManager.towerDataTable.Clear();

        // ==========================================================
        // 1. EnemyData.CSV 파싱
        // ==========================================================

        TextAsset enemyCsv =
            Resources.Load<TextAsset>("EnemyData");

        if (enemyCsv != null)
        {
            string[] lines =
                enemyCsv.text.Split(
                    new[] { "\r\n", "\r", "\n" },
                    System.StringSplitOptions.None
                );

            for (int i = 1; i < lines.Length; i++)
            {
                if (
                    string.IsNullOrWhiteSpace(
                        lines[i]
                    )
                )
                    continue;

                string[] data =
                    lines[i].Split(',');

                // [안전장치]
                // 데이터 개수가 부족하거나
                // 첫 칸이 비어있으면
                // 데이터가 없는 빈 줄이므로 패스
                if (
                    data.Length < 5 ||
                    string.IsNullOrWhiteSpace(
                        data[0].Trim()
                    )
                )
                    continue;

                MonsterStatus status =
                    new MonsterStatus
                    {
                        id =
                            data[0].Trim(),

                        monsterName =
                            data[0].Trim(),

                        hp =
                            float.Parse(
                                data[1].Trim()
                            ),

                        atk =
                            float.Parse(
                                data[2].Trim()
                            ),

                        speed =
                            float.Parse(
                                data[3].Trim()
                            ),

                        rewardGold =
                            int.Parse(
                                data[4].Trim()
                            )
                    };

                dataManager
                    .monsterDataTable
                    .Add(status);
            }

            Debug.Log(
                "EnemyData.CSV 파싱 완료."
            );
        }
        else
        {
            Debug.LogError(
                "Assets/Resources/ 폴더 안에서 'EnemyData' 파일을 찾을 수 없습니다."
            );
        }

        // ==========================================================
        // 2. WaveData.CSV 파싱
        // ==========================================================

        TextAsset waveCsv =
            Resources.Load<TextAsset>("WaveData");

        if (waveCsv != null)
        {
            string[] lines =
                waveCsv.text.Split(
                    new[] { "\r\n", "\r", "\n" },
                    System.StringSplitOptions.None
                );

            for (int i = 1; i < lines.Length; i++)
            {
                if (
                    string.IsNullOrWhiteSpace(
                        lines[i]
                    )
                )
                    continue;

                string[] data =
                    lines[i].Split(',');

                // [안전장치]
                // 데이터 개수가 4개 미만이거나
                // 첫 칸이 비어있으면
                // 빈 줄로 판단하고 패스
                if (
                    data.Length < 4 ||
                    string.IsNullOrWhiteSpace(
                        data[0].Trim()
                    )
                )
                    continue;

                WaveSpawnData wave =
                    new WaveSpawnData
                    {
                        waveIndex =
                            int.Parse(
                                data[0].Trim()
                            ),

                        spawnOrder =
                            int.Parse(
                                data[1].Trim()
                            ),

                        monsterId =
                            data[2].Trim(),

                        delay =
                            float.Parse(
                                data[3].Trim()
                            )
                    };

                dataManager
                    .waveDataTable
                    .Add(wave);
            }

            Debug.Log(
                "WaveData.CSV 파싱 완료."
            );
        }
        else
        {
            Debug.LogError(
                "Assets/Resources/ 폴더 안에서 'WaveData' 파일을 찾을 수 없습니다."
            );
        }

         //==========================================================
         //3. TowerData.CSV 파싱
         //==========================================================

        
        TextAsset towerCsv =
            Resources.Load<TextAsset>("TowerData");

        if (towerCsv != null)
        {
            string[] lines =
                towerCsv.text.Split(
                    new[] { "\r\n", "\r", "\n" },
                    System.StringSplitOptions.None
                );

            for (int i = 1; i < lines.Length; i++)
            {
                if (
                    string.IsNullOrWhiteSpace(
                        lines[i]
                    )
                )
                    continue;

                string[] data =
                    lines[i].Split(',');

                 //[안전장치]
                 //타워 CSV는 16개 컬럼 필요
                 //부족하거나 ID 비면 무시
                if (
                    data.Length < 16 ||
                    string.IsNullOrWhiteSpace(
                        data[0].Trim()
                    )
                )
                    continue;

                TowerData tower =
                    ScriptableObject
                    .CreateInstance<TowerData>();

                tower.towerID =
                    data[0].Trim();

                tower.towerName =
                    data[1].Trim();

                tower.attackType =
                    data[2].Trim();

                tower.damage =
                    float.Parse(
                        data[3].Trim()
                    );

                tower.attackSpeed =
                    float.Parse(
                        data[4].Trim()
                    );

                tower.range =
                    float.Parse(
                        data[5].Trim()
                    );

                tower.canAttackGround =
                    bool.Parse(
                        data[6].Trim()
                    );

                tower.canAttackFlying =
                    bool.Parse(
                        data[7].Trim()
                    );

                tower.canDetectStealth =
                    bool.Parse(
                        data[8].Trim()
                    );

                tower.maxUpgradeLevel =
                    int.Parse(
                        data[9].Trim()
                    );

                tower.upgradeDamagePercent =
                    float.Parse(
                        data[10].Trim()
                    );

                tower.upgradeAttackSpeedPercent =
                    float.Parse(
                        data[11].Trim()
                    );

                tower.upgradeRangePercent =
                    float.Parse(
                        data[12].Trim()
                    );

                 //AttackData 연결
                if (
                    !string.IsNullOrWhiteSpace(
                        data[13]
                    )
                )
                {
                    tower.attackData =
                        Resources.Load<TowerAttackData>(
                            data[13].Trim()
                        );
                }

                 //SupportData 연결
                if (
                    !string.IsNullOrWhiteSpace(
                        data[14]
                    )
                )
                {
                    tower.supportData =
                        Resources.Load<TowerSupportData>(
                            data[14].Trim()
                        );
                }

                 //EconomyData 연결
                if (
                    !string.IsNullOrWhiteSpace(
                        data[15]
                    )
                )
                {
                    tower.economyData =
                        Resources.Load<TowerEconomyData>(
                            data[15].Trim()
                        );
                }

                dataManager
                    .towerDataTable
                    .Add(tower);
            }

            Debug.Log(
                "TowerData.CSV 파싱 완료."
            );
        }
        else
        {
            Debug.LogError(
                "Assets/Resources/ 폴더 안에서 'TowerData' 파일을 찾을 수 없습니다."
            );
        }
        

        EditorUtility.SetDirty(
            dataManager
        );

        AssetDatabase.SaveAssets();

        Debug.Log(
            "🎉 모든 CSV 데이터가 오류 없이 성공적으로 갱신되었습니다!"
        );
    }
}

#endif