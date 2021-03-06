﻿using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ServerStageManager : NetworkBehaviour
{

    EnemySpawner enemy_spawner_ = null;

    bool[] go_room_ = new bool[3];

    List<GameObject> enemys = new List<GameObject>();

    float result_count_ = 0.0f;
    float limit_count_ = 60* 5;

    public bool CanGoRoom(uint num)
    {
        if (num > go_room_.Length) return false;
        return go_room_[num];
    }

    public bool goResult
    {
        get
        {
            if (limit_count_ <= 0.0f) return true;

            var players = GameObject.FindGameObjectsWithTag("Player");
            List<HPManager> player_hp_managers = new List<HPManager>();
            foreach (var player in players)
            {
                player_hp_managers.Add(player.GetComponent<HPManager>());
            }

            if (player_hp_managers.All(player => !player.isActive)) return true;

            List<HPManager> enemy_hp_managers = new List<HPManager>();

            if (enemys.Count <= 17) return false;
            var active_enemys = enemys.GetRange(7, 11);
            foreach (var enemy in active_enemys)
            {
                if (enemy == null) continue;
                enemy_hp_managers.Add(enemy.GetComponent<HPManager>());
            }

            if (enemy_hp_managers.All(enemy => !enemy.isActive)) return true;
            return false;
        }
    }

    public bool isWinPlayer
    {
        get
        {
            if (limit_count_ <= 0.0f) return false;

            foreach (var player in FindObjectsOfType<PlayerSetting>())
            {
                var hp_manager = player.GetComponent<HPManager>();
                if (hp_manager.isActive) return true;
            }
            return false;
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        enemy_spawner_ = FindObjectOfType<EnemySpawner>();

        for (int i = 0; i < 1; i++)
        {
            enemys.Add(enemy_spawner_.CreateEnemy(i));
        }

        StartCoroutine("Room1");
    }

    void Update()
    {
        var go_result = goResult;
        Result(go_result);
        LimitTimeUpdate(go_result);
    }

    void Result(bool go_result)
    {
        if (!go_result) return;

        if (result_count_ <= 0.0)
        {
            if (isWinPlayer)
            {
                //　勝利演出

                foreach (var player in FindObjectsOfType<EndDirector>())
                {
                    player.RpcTellClientStart("Win");
                }
            }
            else
            {
                //　敗北演出
                foreach (var player in FindObjectsOfType<EndDirector>())
                {
                    player.RpcTellClientStart("Lose");
                }
            }
        }

        result_count_ += Time.deltaTime;

        if (result_count_ <= 5.0f) return;

        FindObjectOfType<MyNetworkLobbyManager>().StopHost();
    }

    void LimitTimeUpdate(bool go_result)
    {
        if (go_result) return;
        limit_count_ -= Time.deltaTime;

        foreach (var player in FindObjectsOfType<Limiter>())
        {
            player.RpcTellToClient(limit_count_);
        }
    }

    IEnumerator Room1()
    {
        while (true)
        {
            if (enemys[0].GetComponent<HPManager>().isActive)
            {
                yield return null;
            }
            else
            {
                if (go_room_[0]) yield return null;
                for (int i = 1; i < 3; i++)
                {
                    enemys.Add(enemy_spawner_.CreateEnemy(i));
                }
                go_room_[0] = true;
                yield return StartCoroutine("Room2");
            }

        }
    }

    IEnumerator Room2()
    {
        while (true)
        {
            var enemys_active = false;

            var count = 0;
            for (int i = 1; i < 3; i++)
            {
                if (enemys[i] != null) break;
                count++;
            }

            if (count == 2) enemys_active = true;

            if (enemys_active)
            {
                if (go_room_[1]) yield return null;

                for (int i = 3; i < 8; i++)
                {
                    enemys.Add(enemy_spawner_.CreateEnemy(i));
                }
                go_room_[1] = true;
                yield return StartCoroutine("Room3");
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator Room3()
    {
        while (true)
        {
            var enemys_active = false;

            var count = 0;
            for (int i = 3; i < 8; i++)
            {
                if (enemys[i] != null) break;
                count++;
            }

            if (count == 5) enemys_active = true;

            if (enemys_active)
            {
                if (go_room_[2]) yield return null;

                for (int i = 8; i < 18; i++)
                {
                    enemys.Add(enemy_spawner_.CreateEnemy(i));
                }
                go_room_[2] = true;
                StopCoroutine("Room3");
            }

            yield return null;
        }
    }
}
