# CZCIT Solar Glare

给太阳方向叠加摄影星芒、光晕和镜头鬼影。Shader 绘制图形，C# 负责取得太阳方向、判断遮挡和控制相机上的叠加层。

1. 在 Built-in 工程导入包，给透视相机添加 **Solar Glare Camera**。
2. 创建 Directional Light，拖到组件的 **Sun Override**，也可以在 Lighting 中把它设为 **Sun Source**。
3. 朝光源方向看，用 **Intensity** 调强度。用带 Collider 的立方体挡在相机与太阳方向之间，可观察光效消失。遮挡物需要在相机的 Culling Mask 内。
4. 若玩家碰撞体挡住光效，把玩家根节点拖到 **Ignore Occluders Under**。相机需渲染 **Ignore Raycast** 层，叠加层使用该层。

包内也带有可供自定义天空 Shader 调用的 SolarLensFlare.cginc。相机效果和天空 Shader 路径按需选用一种，避免重复叠加。
