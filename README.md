# Default Unity 2022 — 0.1
Unity 2022.3.43f1
  
# Assets list
## Third-party
### Plugins
Amplify Shader Editor — 1.9.2.2  
Beautify 3 — 22.3.1  
Lean GUI — 2.1  
DOTween Pro — 1.0.375  
Easy Save — 3.5.16  
Fast script reload — 1.6  
Text Animator — 2.0.2  
Feel — 4.2  
Horizon Based Ambient Occlusion — 3.5  
Odin Inspector, Serializer, Validator — 3.3.1.7  
Rewired — 1.1.47.0  
Reflex — 9  


### Editor 
Build Report — 3.10.1  
Code Todo List — 1.1  
Editor Console Pro — 3.975  
Shaders Limiter — 1.2  
Fullscreen Editor — 2.2.8  
Hierarchy 4 — 1.4.2.1  
Hierarchy Pro — Extended 2022.1.4  
Perfect Focus — 1.6  
Rainbow Folders — 2.4.1  
UModeler — 2.11.8  
SuperPivot  
  
### Content  
Amplify Shader Pack  
  
# Project structure
Here I used folder structure by Feature  
Learn more about сontent based vs feature based sructure  
```
Assets
|---Source					Your content and features
| 	|---Common				Things that are used in multiple features
|		|---Settings			User settings and configuration files
| 			|---Volume
| 			|---Presets
| 			|---Quality
| 			|---Renderer
| 			|---ShaderVariants
|
|---Resources			Some assets store their settings here. For example DOTween
|
|---Third-party			Third-party content from the Asset Store
| 	|---Content		Any art-related asset with its own structure that does not bring additional functionality
| 	|---Editor		Any editor extensions that should not affect the build
| 	|---Plugins		Other third-party assets that bring new functionality to the build (usually)
|
|---zzzTrash			Files to be assigned or deleted

```


# Changelog

---

## [0.1] — 2024-09-07