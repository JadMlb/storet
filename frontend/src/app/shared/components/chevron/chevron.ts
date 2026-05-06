import { Component, computed, input, Input } from '@angular/core';

const ROTATION_ANGLES = {
  "right": 0,
  "top": 270,
  "bottom": 90,
  "left": 180
};

@Component ({
  selector: 'chevron',
  imports: [],
  templateUrl: './chevron.html',
  styleUrl: './chevron.scss',
})
export class Chevron
{
  rotation = input<"left" | "right" | "top" | "bottom"> ("bottom");
  
  style = computed (
    () =>
    {
      const rotation = this.rotation();
      return {
        transform: `rotate(${ROTATION_ANGLES[rotation]}deg)`
      }
    }
  );
}