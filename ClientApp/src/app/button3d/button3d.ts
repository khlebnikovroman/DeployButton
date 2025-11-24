import { Component, ElementRef, OnDestroy, ViewChild } from '@angular/core';
import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-button-3d',
  templateUrl: './button3d.html',
  styleUrls: ['./button3d.css'],
  imports: [ MatCardModule]
})
export class Button3dComponent implements OnDestroy {
  @ViewChild('container') container!: ElementRef;

  private scene!: THREE.Scene;
  private camera!: THREE.PerspectiveCamera;
  private renderer!: THREE.WebGLRenderer;
  private animationId!: number;
  private controls!: OrbitControls;
  ngAfterViewInit() {
    this.initThree();
    this.animate();
  }

  private initThree() {
    // Сцена
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color(0xffffff);

    // Камера
    this.camera = new THREE.PerspectiveCamera(
      75,
      1,
      1,
      1000
    );
    this.camera.position.set(100, 0, 5);

    // Рендерер
    this.renderer = new THREE.WebGLRenderer({ antialias: true });
    this.container.nativeElement.appendChild(this.renderer.domElement);
// После this.renderer = ... и this.scene.add(light)
    this.controls = new OrbitControls(this.camera, this.renderer.domElement);
    this.controls.enablePan = false;
    this.controls.enableDamping = true; // плавность
    this.controls.dampingFactor = 0.05;
    this.controls.screenSpacePanning = false;
    this.controls.minDistance = 120;
    this.controls.maxDistance = 200;
    this.controls.update();
    // Свет
    const light = new THREE.DirectionalLight(0xffffff, 1);
    light.position.set(5, 5, 5);
    this.scene.add(light);
    this.scene.add(new THREE.AmbientLight(0xffffff, 0.5));

    // Загрузка модели
    const loader = new GLTFLoader();
    loader.load('/buttonanimated.glb', (gltf) => {
      // Центрируем модель
      const box = new THREE.Box3().setFromObject(gltf.scene);
      const center = box.getCenter(new THREE.Vector3());
      gltf.scene.position.x = -center.x;
      gltf.scene.position.y = -center.y;
      gltf.scene.position.z = -center.z;

      // Масштабируем (если нужно)
      // gltf.scene.scale.set(0.5, 0.5, 0.5);

      this.scene.add(gltf.scene);
    });

    // Обработка изменения размера окна
    this.onWindowResize()
    window.addEventListener('resize', this.onWindowResize);
  }

  private onWindowResize = () => {
    const container = this.container.nativeElement;
    const width = container.clientWidth;
    const height = container.clientHeight;

    // Обновляем размеры
    this.camera.aspect = width / height;
    this.camera.updateProjectionMatrix();
    this.renderer.setSize(width, height);
  };

  private animate = () => {
    this.animationId = requestAnimationFrame(this.animate);
    this.renderer.render(this.scene, this.camera);
    this.controls.update(); // ← важно!
  };

  ngOnDestroy() {
    cancelAnimationFrame(this.animationId);
    window.removeEventListener('resize', this.onWindowResize);
    this.renderer?.dispose();
  }
}
