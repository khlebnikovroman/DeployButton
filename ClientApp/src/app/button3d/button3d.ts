import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { HDRLoader } from 'three/examples/jsm/loaders/HDRLoader.js';
import { MatCardModule } from '@angular/material/card';
import { DeviceStateService } from '../shared/services/device-state-service'
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-button-3d',
  templateUrl: './button3d.html',
  styleUrls: ['./button3d.css'],
  imports: [MatCardModule]
})
export class Button3dComponent implements OnDestroy, OnInit {
  @ViewChild('container') container!: ElementRef;

  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private animationId!: number;
  private controls!: OrbitControls;
  private mixer!: THREE.AnimationMixer;
  private animationSubscription$: Subscription | null = null;
  private clock = new THREE.Clock();
  private gltfAnimations: THREE.AnimationClip[] = [];
  /**
   *
   */
  constructor(private deviceStateService: DeviceStateService) {}
  ngOnInit() {
    this.animationSubscription$ = this.deviceStateService.getPressedState().subscribe(() => {
      this.playPressAnimation();
    });
  }

  ngAfterViewInit() {
    this.initThree();
    this.render();
  }

  private initThree() {
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0xffffff);

    this.camera = new THREE.PerspectiveCamera(75, 1, 1, 1000);
    this.camera.position.set(100, 0, 5);

    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.container.nativeElement.appendChild(this.renderer.domElement);

    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enablePan = false;
    this.controls.enableDamping = true;
    this.controls.dampingFactor = 0.05;
    this.controls.screenSpacePanning = false;
    this.controls.minDistance = 120;
    this.controls.maxDistance = 200;

    // Свет
    const light = new THREE.DirectionalLight(0xffffff, 1);
    light.position.set(5, 5, 5);
    this.scene.add(light);
    this.scene.add(new THREE.AmbientLight(0xffffff, 0.5));

    // Загрузка HDR и модели
    const cubemap = new HDRLoader().load("hdri.hdr", (texture) => {
      texture.mapping = THREE.EquirectangularReflectionMapping;
      this.scene.environment = texture;

      const loader = new GLTFLoader();
      loader.load('buttonanimated.glb', async (gltf) => {
      const model = gltf.scene;
      this.scene.add(model);

      // Сохраняем анимации!
      this.gltfAnimations = gltf.animations;

      this.mixer = new THREE.AnimationMixer(model);

      this.onWindowResize();
    });;
    });

    this.onWindowResize();
    window.addEventListener('resize', this.onWindowResize);
  }

  private onWindowResize = () => {
    const container = this.container.nativeElement;
    const width = container.clientWidth;
    const height = container.clientHeight;
    this.camera.aspect = width / height;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(width, height);
  };

  private render = () => {
    this.animationId = requestAnimationFrame(this.render);
    const delta = this.mixer ? this.clock.getDelta() : 0;
    if (this.mixer) this.mixer.update(delta);
    this.renderer.render(this.scene, this.camera);
    this.controls.update();
  };

  // Метод для проигрывания анимации нажатия
  public playPressAnimation() {
    if (!this.mixer || this.gltfAnimations.length === 0) {
      console.warn('No animations available');
      return;
    }

    const clip = this.gltfAnimations[0]; // первая анимация
    const action = this.mixer.clipAction(clip);
    action.reset().play();

    // Остановка после завершения (опционально)
    action.clampWhenFinished = true;
    action.setLoop(THREE.LoopOnce, 1);
  }

  ngOnDestroy() {
    this.animationSubscription$?.unsubscribe();
    cancelAnimationFrame(this.animationId);
    window.removeEventListener('resize', this.onWindowResize);
    this.renderer?.dispose();
  }
}