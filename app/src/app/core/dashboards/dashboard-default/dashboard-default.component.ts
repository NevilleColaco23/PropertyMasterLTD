import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-dashboard-default',
  templateUrl: './dashboard-default.component.html',
  styleUrl: './dashboard-default.component.css'
})
export class DashboardDefaultComponent implements OnInit {
  imageUrl: string = '';

  private imageUrls: string[] = [
    'https://picsum.photos/seed/pic1/600/400',
    'https://picsum.photos/seed/pic2/600/400',
    'https://picsum.photos/seed/pic3/600/400',
    'https://picsum.photos/seed/pic4/600/400',
    'https://picsum.photos/seed/pic5/600/400'
  ];

  ngOnInit(): void {
    this.showRandomImage();
  }

  showRandomImage() {
    const randomIndex = Math.floor(Math.random() * this.imageUrls.length);
    this.imageUrl = this.imageUrls[randomIndex];
  }
}