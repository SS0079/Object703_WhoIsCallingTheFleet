# KittyHelpYouOutToolBox
 一个私人小工具集合

## 目录
### **Prefab**
- ShowMinimapViewScopeComponent：一个实现RTS小地图中摄像机范围显示的组件，通过数学方法直接算出摄像机的四个角的延长线在指定高度水平面的的交点，然后用LineRenderer连接起来。ViewScopeRenderer是它的Prefab
### **ServiceClass**
#### ChildrenHelper：包含Game Object获取children的若干扩展方法 （需要改进）
1. gameObject.GetComponentsOnlyInChildren()：和原生的GetComponentsInChildren类似，但会自动排除gameObject自己。
2. component.GetComponentsOnlyInChildren()：上述方法的重载
3. gameObject.GetNamedInChildren(string name)：按gameObject.name查找Children

#### DebugLogOutput （需要大改）
#### DeltaCalculator：用来计算每次输入和前次输入插值的容器，目前可以计算float和Vector3的角度差。好处在于能少打几行代码
#### IniParser：Ini文件读写器，抄来的，正在修改
#### JsonFileHandler：Json文件读写器
