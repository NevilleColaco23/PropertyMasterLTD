import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GroupsService } from '../../../../profile/groups.service';
import { Group } from '../../../../profile/post.model';

@Component({
  selector: 'app-group-master',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './group-master.component.html',
  styleUrl: './group-master.component.css'
})
export class GroupMasterComponent implements OnInit, AfterViewInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns: string[] = ['id', 'name', 'description', 'actions'];
  dataSource = new MatTableDataSource<Group>([]);

  isLoading = false;

  showNewGroupForm = false;
  newGroupName = '';
  newGroupDescription = '';

  editingGroupId: number | null = null;
  editGroupName = '';
  editGroupDescription = '';

  constructor(private groupsService: GroupsService) {}

  ngOnInit(): void {
    this.groupsService.groups$.subscribe(groups => {
      this.dataSource.data = groups;
    });
    this.groupsService.refresh();
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.dataSource.filter = value.trim().toLowerCase();
  }

  createGroup(): void {
    if (!this.newGroupName.trim()) {
      return;
    }

    this.isLoading = true;
    this.groupsService.addGroup(this.newGroupName, this.newGroupDescription).subscribe(() => {
      this.newGroupName = '';
      this.newGroupDescription = '';
      this.showNewGroupForm = false;
      this.isLoading = false;
    });
  }

  startEditGroup(group: Group): void {
    this.editingGroupId = group.id;
    this.editGroupName = group.name;
    this.editGroupDescription = group.description;
  }

  cancelEditGroup(): void {
    this.editingGroupId = null;
    this.editGroupName = '';
    this.editGroupDescription = '';
  }

  saveEditGroup(): void {
    if (this.editingGroupId == null || !this.editGroupName.trim()) {
      return;
    }

    this.isLoading = true;
    this.groupsService.updateGroup(this.editingGroupId, this.editGroupName, this.editGroupDescription).subscribe(() => {
      this.cancelEditGroup();
      this.isLoading = false;
    });
  }

  deleteGroup(group: Group): void {
    if (!confirm(`Delete group "${group.name}"?`)) {
      return;
    }

    this.isLoading = true;
    this.groupsService.deleteGroup(group.id).subscribe(() => {
      if (this.editingGroupId === group.id) {
        this.cancelEditGroup();
      }
      this.isLoading = false;
    });
  }
}
