﻿'------------------------------------------------------------------------------
' <auto-generated>
'     此代码由工具生成。
'     运行时版本:4.0.30319.42000
'
'     对此文件的更改可能会导致不正确的行为，并且如果
'     重新生成代码，这些更改将会丢失。
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    '此类是由 StronglyTypedResourceBuilder
    '类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    '若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    '(以 /str 作为命令选项)，或重新生成 VS 项目。
    '''<summary>
    '''  一个强类型的资源类，用于查找本地化的字符串等。
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  返回此类使用的缓存的 ResourceManager 实例。
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("p104.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  重写当前线程的 CurrentUICulture 属性
        '''  重写当前线程的 CurrentUICulture 属性。
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  查找类似 &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        '''&lt;content&gt;
        '''  &lt;h id=&quot;0&quot; template=&quot;0&quot;&gt;
        '''    &lt;level value=&quot;1&quot; /&gt;
        '''    &lt;lock value=&quot;Unlocked&quot; /&gt;
        '''  &lt;/h&gt;
        '''  &lt;h id=&quot;1&quot; template=&quot;1&quot; &gt;
        '''    &lt;level value=&quot;1&quot; /&gt;
        '''    &lt;lock value=&quot;Unlocked&quot; /&gt;
        '''  &lt;/h&gt;
        '''&lt;/content&gt; 的本地化字符串。
        '''</summary>
        Friend ReadOnly Property AllUnits() As String
            Get
                Return ResourceManager.GetString("AllUnits", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  查找类似 &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        '''&lt;content&gt;
        '''  &lt;static id=&quot;0&quot; group=&quot;skirmish_chess&quot; domain=&quot;fine&quot; &gt;
        '''    &lt;fragment id=&quot;0&quot; from=&quot;skirmish_unit&quot; pos=&quot;0,0&quot; size=&quot;-1,-1&quot;/&gt;
        '''  &lt;/static&gt;
        '''&lt;/content&gt; 的本地化字符串。
        '''</summary>
        Friend ReadOnly Property AssembleImages() As String
            Get
                Return ResourceManager.GetString("AssembleImages", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  查找类似 &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        '''&lt;content&gt;
        '''  &lt;set index=&quot;0&quot;&gt;
        '''    &lt;base value=&quot;255,255,64,89&quot;/&gt;
        '''    &lt;light value=&quot;191,255,191,204&quot;/&gt;
        '''    &lt;dark value=&quot;255,179,77,85&quot;/&gt;
        '''    &lt;based1 value=&quot;0,0,0,0&quot;/&gt;
        '''    &lt;basel1 value=&quot;0,0,0,0&quot;/&gt;
        '''    &lt;lightd1 value=&quot;255,204,140,166&quot;/&gt;
        '''    &lt;lightl1 value=&quot;0,0,0,0&quot;/&gt;
        '''    &lt;darkd1 value=&quot;0,0,0,0&quot;/&gt;
        '''    &lt;darkl1 value=&quot;0,0,0,0&quot;/&gt;
        '''  &lt;/set&gt;
        '''&lt;/content&gt; 的本地化字符串。
        '''</summary>
        Friend ReadOnly Property Colours() As String
            Get
                Return ResourceManager.GetString("Colours", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  查找类似 &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        '''&lt;content&gt;
        '''  &lt;side value=&quot;0&quot; name=&quot;Remilia&quot; type=&quot;Player&quot; team=&quot;0&quot;&gt;
        '''    &lt;character&gt;
        '''      &lt;hero w_index=&quot;1&quot;&gt;
        '''        &lt;startpos value=&quot;0,0,0&quot;/&gt;
        '''      &lt;/hero&gt;
        '''      
        '''    &lt;/character&gt;
        '''    &lt;building&gt;
        '''      
        '''    &lt;/building&gt;
        '''    &lt;giventech&gt;
        '''      
        '''    &lt;/giventech&gt;
        '''    &lt;endgame&gt;
        '''      &lt;win&gt;
        '''        &lt;winevent name=&quot;Clean&quot; value=&quot;0&quot;/&gt;
        '''      &lt;/win&gt;
        '''      &lt;lose&gt;
        '''        &lt;loseevent name=&quot;Clean&quot; value=&quot;0&quot;/&gt;
        '''        &lt;loseevent name=&quot;Turn&quot; value=&quot;20&quot; /&gt;
        '''      &lt;/l [字符串的其余部分被截断]&quot;; 的本地化字符串。
        '''</summary>
        Friend ReadOnly Property MapScriptTest() As String
            Get
                Return ResourceManager.GetString("MapScriptTest", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  查找类似 &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        '''&lt;content&gt;
        '''  &lt;heroskill&gt;
        '''    &lt;skill id=&quot;0&quot; name=&quot;澶╃┖涓缈旂殑宸コ&quot; parent=&quot;-1&quot; user=&quot;Reimu&quot; type=&quot;Buff&quot; stage=&quot;1&quot; cost=&quot;2&quot;&gt;
        '''      &lt;buff target=&quot;Reimu&quot; side=&quot;Self&quot; name=&quot;move&quot; value=&quot;1&quot;/&gt;
        '''      &lt;buff target=&quot;Reimu&quot; side=&quot;Self&quot; name=&quot;movetype&quot; value=&quot;SkyBase&quot;/&gt;
        '''    &lt;/skill&gt;
        '''    &lt;skill id=&quot;1&quot; name=&quot;鍗氫附绁炵ぞ鐨勫帆濂? parent=&quot;-1&quot; user=&quot;Reimu&quot; type=&quot;Buff&quot; stage=&quot;1&quot; cost=&quot;2&quot;&gt;
        '''      &lt;buff target=&quot;Reimu&quot; side=&quot;Self&quot; name=&quot;damageto&quot; receiver=&quot;Youkai&quot; value=&quot;20p&quot;/&gt;
        '''    &lt;/skill&gt;
        '''   [字符串的其余部分被截断]&quot;; 的本地化字符串。
        '''</summary>
        Friend ReadOnly Property Skill() As String
            Get
                Return ResourceManager.GetString("Skill", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  查找类似 &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        '''
        '''&lt;content&gt;
        '''  &lt;ut index=&quot;0&quot; name=&quot;毛玉&quot; unittype=&quot;Kedama&quot; hp=&quot;10&quot; move=&quot;LandAndWater,5&quot; burden=&quot;1&quot; view=&quot;2&quot; hide=&quot;14&quot;&gt;
        '''    &lt;attack damage=&quot;Physical&quot; value=&quot;5,7&quot; atkrange=&quot;1,1&quot; hitrange=&quot;1,1&quot; /&gt;
        '''    &lt;defend value=&quot;0&quot; resistance=&quot;0.1,0.1,0&quot;/&gt;
        '''  &lt;/ut&gt;
        '''  
        '''  &lt;ut index=&quot;1&quot; name=&quot;妖精&quot; unittype=&quot;Yousei&quot; hp=&quot;20&quot; move=&quot;LandAndWater,5&quot; burden=&quot;2&quot; view=&quot;2&quot; hide=&quot;12&quot;&gt;
        '''    &lt;attack damage=&quot;Physical&quot; value=&quot;9,13&quot; atkrange=&quot;1,1&quot; hitrange=&quot;1,1&quot; /&gt;
        '''    &lt;defend value=&quot;2&quot; resistance= [字符串的其余部分被截断]&quot;; 的本地化字符串。
        '''</summary>
        Friend ReadOnly Property UnitTemplates() As String
            Get
                Return ResourceManager.GetString("UnitTemplates", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
