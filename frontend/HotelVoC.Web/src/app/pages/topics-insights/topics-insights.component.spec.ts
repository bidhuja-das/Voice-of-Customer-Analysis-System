import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TopicsInsightsComponent } from './topics-insights.component';

describe('TopicsInsightsComponent', () => {
  let component: TopicsInsightsComponent;
  let fixture: ComponentFixture<TopicsInsightsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TopicsInsightsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TopicsInsightsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
