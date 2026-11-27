using UnityEngine;

public static class CEnemyBoundRenderer
{
    private static Bounds GetEntityBound(CEnemy enemy)
    {
        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>();
        bool init = false;

        Bounds bounds = default;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || !renderers[i].enabled)
            {
                continue;
            }

            if (!init)
            {
                bounds = renderers[i].bounds;
                init = true;
                continue;
            }

            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    public static bool TryRayHit(Ray ray, CEnemy enemy)
    {
        Bounds bounds = GetEntityBound(enemy);

        if (bounds == default)
        {
            return false;
        }

        bool hit = bounds.IntersectRay(ray);

        return hit;
    }
}
