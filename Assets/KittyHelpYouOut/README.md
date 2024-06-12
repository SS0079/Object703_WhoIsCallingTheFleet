# KittyHelpYouOutToolBox
 一个私人小工具集合

## 目录
### **Prefab**
#### ShowMinimapViewScopeComponent：一个实现RTS小地图中摄像机范围显示的组件，通过数学方法直接算出摄像机的四个角的延长线在指定高度水平面的的交点，然后用LineRenderer连接起来。ViewScopeRenderer是它的Prefab

### **ServiceClass**
#### ChildrenHelper：包含Game Object获取children的若干扩展方法 （需要改进）
#### DebugLogOutput （需要大改）
#### DeltaCalculator：用来计算每次输入和前次输入插值的容器，目前可以计算float和Vector3的角度差。好处在于能少打几行代码
#### IniParser：Ini文件读写器，抄来的，正在修改
#### JsonFileHandler：Json文件读写器
#### KittyBindDictionary：可绑定的字典容器，增删改查皆可绑定事件。在不想触发事件的场合，还提供Silence方法
#### KittyBindHashSet：可绑定事件的哈希表，同样提供Silence方法
#### KittyRingBuffer：环状缓存，支持以头尾为起点的增删改查，并且会自动扩容
#### KittyCoroutine：一个单例，用来在KYHO工具箱内部运行协程，没有业务逻辑
#### KittyCortUDPListener：一个使用协程来监听UDP的工具类
#### KittyMonoSingletonAuto：单例基类，在没有被创建时访问会自动创建
#### KittyMonoSingletonManual：同上，但是需要手动创建
#### KittyPool：一个主要伺服GameObject的对象池，在回收对象时可以选择是hide还是set inactive
#### KittyStrategyMachine：忘了这个是什么了，之后仔细看看
#### KittyTimer：需要在Game loop 中手动tick的计时器。好处是不需要协程，而且少写几行代码
#### KittyDataEntity：数据实体容器，每次更改时会更新自己的版本，通过对比版本来决定某个值是否需要更新，用来做数据驱动
#### KittyEvent：仿照QF写的基于类型的事件
#### KittyInputHandler：一个方便序列化按键绑定设定的输入配置器。没写完
#### KittyLoomUDPClient：一个用多线程监听UDP的工具类
#### ListHelper：包含若干服务List的扩展方法
#### LoopIndex：一个方便index在指定范围loop取值的工具类。方便少写几行代码
#### OpenFile：用来弹出资源管理器窗口选择文件的静态方法。之前AI写的RAS2的OpenFile似乎更好，记得对比更新一下
#### QuadrangularPrismCollider：一个生成四棱锥网格的工具类。抄的，需要仔细研究一下
#### RegexPatterns：一些记录常用正则表达式的静态字段
#### StringHelper：若干服务string的扩展方法。方便少打几行代码
#### UnityEditorHelper：若干方便编写UnityEditor的扩展方法

### **Utilities**
#### AbstractPerformanceAnalyzer：一个用来简单现实FPS的抽象MonoBehaviour，使用时继承
#### AttachToWheelCollider：将车轮模型绑定到WheelCollider上
#### CarPhysicsAntiRoll：车辆防滚架
#### CruiseMove：俯瞰视角移动器
#### CustomSlider：自定义的Slider，分离了OnDrag，OnPointerDown和OnPointerUp三个事件
#### CustomToggle：自定义的Toggle，分离了OnClick事件
#### DebugController：debug面板，将部分信息用OnGUI打印到屏幕上
#### FaceCamera：迫使挂载的GameObject面向摄像头。可以指定在某一Game loop阶段执行，或者手动执行
#### FreeMove：自由移动
#### InstallerBase：Installer抽象MonoBehaviour和相关的InstallComponent特性，和RequireComponent差别不大，但挂起所需的组件后会删除自己。使用时继承
#### KittyMath：数学类，包括各种换算、算法等。有空在下面列举一下，还挺重要的
#### MainCameraUtility：服务相机的静态工具类。有些内容因为Unity更新已经没有用了。包含一个从屏幕发射射线的方法。
#### MergeMesh：融合网格。似乎有问题，有空研究一下
#### MeshFixTool：记不得这是做什么用的了。有空仔细看看
#### Orbiter：使挂载对象环绕一个目标的组件。同样可以指定在哪个阶段执行或手动执行
#### ReadOnlyInspectorAttribute：其实和QF的一样
#### ScaleMeter：用来测量网格在场景中的实际尺寸
#### ScriptTemplateCtrl：代码模板生成器。没用了，但值得研究
#### TakeCameraShot：通过场景摄像机截图的工具类
#### TriggerBox：盒子触发器。没什么用就删了吧
#### UIDrag：允许拖拽UI的组件
#### UnifiedTransformControl：统一变换控制。方便做一些中心对称的动画

